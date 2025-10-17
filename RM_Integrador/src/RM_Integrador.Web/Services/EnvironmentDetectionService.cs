using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Net;

namespace RM_Integrador.Web.Services
{
    public interface IEnvironmentDetectionService
    {
        Task<bool> IsProductionEnvironmentAsync();
        Task<bool> IsLocalRMAvailableAsync();
        Task<List<int>> GetAvailableRMPortsAsync();
        Task<string> DetectBestRMUrlAsync();
        EnvironmentInfo GetEnvironmentInfo();
    }

    public class EnvironmentInfo
    {
        public bool IsProduction { get; set; }
        public bool IsLocalRMAvailable { get; set; }
        public List<int> AvailableRMPorts { get; set; } = new();
        public string RecommendedRMUrl { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public DateTime DetectionTime { get; set; }
        public string DetectionDetails { get; set; } = string.Empty;
    }

    public class EnvironmentDetectionService : IEnvironmentDetectionService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnvironmentDetectionService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private EnvironmentInfo? _cachedInfo;
        private DateTime _lastDetection = DateTime.MinValue;
        private readonly TimeSpan _cacheValidTime = TimeSpan.FromMinutes(5);
        private readonly object _lockObject = new object();

        public EnvironmentDetectionService(
            IConfiguration configuration,
            ILogger<EnvironmentDetectionService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public EnvironmentInfo GetEnvironmentInfo()
        {
            lock (_lockObject)
            {
                if (_cachedInfo != null && DateTime.Now - _lastDetection < _cacheValidTime)
                {
                    return _cachedInfo;
                }

                // Executa detecção síncrona básica
                try
                {
                    var task = Task.Run(async () => await DetectEnvironmentAsync());
                    task.Wait(TimeSpan.FromSeconds(10)); // Timeout de 10 segundos
                    
                    return task.Result ?? CreateSafeDefaultInfo();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro na detecção de ambiente, usando valores padrão");
                    return CreateSafeDefaultInfo();
                }
            }
        }

        private EnvironmentInfo CreateSafeDefaultInfo()
        {
            return new EnvironmentInfo
            {
                IsProduction = true, // Safe default
                Environment = "Unknown",
                MachineName = Environment.MachineName,
                DetectionTime = DateTime.Now,
                DetectionDetails = "Detection failed - using safe defaults",
                RecommendedRMUrl = _configuration["RMSettings:BaseUrl"] ?? "http://localhost:8051/rmsrestdataserver/rest"
            };
        }

        public async Task<bool> IsProductionEnvironmentAsync()
        {
            var info = await DetectEnvironmentAsync();
            return info.IsProduction;
        }

        public async Task<bool> IsLocalRMAvailableAsync()
        {
            var info = await DetectEnvironmentAsync();
            return info.IsLocalRMAvailable;
        }

        public async Task<List<int>> GetAvailableRMPortsAsync()
        {
            var info = await DetectEnvironmentAsync();
            return info.AvailableRMPorts;
        }

        public async Task<string> DetectBestRMUrlAsync()
        {
            // Força uma nova detecção limpando o cache
            lock (_lockObject)
            {
                _cachedInfo = null;
            }
            
            var info = await DetectEnvironmentAsync();
            return info.RecommendedRMUrl;
        }

        private async Task<EnvironmentInfo> DetectEnvironmentAsync()
        {
            // Verifica cache novamente dentro do método async
            if (_cachedInfo != null && DateTime.Now - _lastDetection < _cacheValidTime)
            {
                return _cachedInfo;
            }

            var info = new EnvironmentInfo
            {
                MachineName = Environment.MachineName,
                DetectionTime = DateTime.Now
            };

            var details = new List<string>();

            try 
            {
                // 1. Detectar ambiente baseado em configurações
                var aspNetCoreEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var isDevelopment = aspNetCoreEnv?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true;
                var envSettings = _configuration.GetSection("EnvironmentSettings");
                var configIsProduction = envSettings.GetValue<bool>("IsProduction", !isDevelopment);
                
                info.IsProduction = configIsProduction;
                info.Environment = aspNetCoreEnv ?? (configIsProduction ? "Production" : "Development");
                
                details.Add($"ASPNETCORE_ENVIRONMENT: {aspNetCoreEnv ?? "not set"}");
                details.Add($"Configuration IsProduction: {configIsProduction}");
                details.Add($"Final Environment: {info.Environment}");

                // 2. Detectar portas RM disponíveis
                var configuredPorts = envSettings.GetSection("LocalRMPorts").Get<int[]>() ?? new[] { 8051, 8050, 8052, 8053 };
                var timeout = envSettings.GetValue<int>("DetectionTimeout", isDevelopment ? 5000 : 2000);
                
                info.AvailableRMPorts = await DetectAvailablePortsAsync(configuredPorts, timeout);
                info.IsLocalRMAvailable = info.AvailableRMPorts.Any();
                
                details.Add($"Tested ports: {string.Join(", ", configuredPorts)}");
                details.Add($"Available ports: {string.Join(", ", info.AvailableRMPorts)}");
                details.Add($"Local RM available: {info.IsLocalRMAvailable}");

                // 3. Determinar URL recomendada
                if (info.IsLocalRMAvailable && !info.IsProduction)
                {
                    var bestPort = info.AvailableRMPorts.First();
                    info.RecommendedRMUrl = $"http://localhost:{bestPort}/rmsrestdataserver/rest";
                    details.Add($"Using local RM on port {bestPort}");
                }
                else
                {
                    info.RecommendedRMUrl = _configuration["RMSettings:BaseUrl"] ?? "http://localhost:8051/rmsrestdataserver/rest";
                    details.Add($"Using configured RM URL: {info.RecommendedRMUrl}");
                }

                info.DetectionDetails = string.Join("; ", details);
                _logger.LogInformation($"Environment detection completed: {info.DetectionDetails}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during environment detection");
                info.DetectionDetails = $"Detection error: {ex.Message}";
                
                // Safe defaults on error
                info.IsProduction = true;
                info.IsLocalRMAvailable = false;
                info.Environment = "Unknown";
                info.RecommendedRMUrl = _configuration["RMSettings:BaseUrl"] ?? "http://localhost:8051/rmsrestdataserver/rest";
            }

            // Cache os resultados
            lock (_lockObject)
            {
                _cachedInfo = info;
                _lastDetection = DateTime.Now;
            }
            
            return info;
        }

        private async Task<List<int>> DetectAvailablePortsAsync(int[] ports, int timeoutMs)
        {
            var availablePorts = new List<int>();
            var tasks = ports.Select(port => CheckPortAsync(port, timeoutMs));
            var results = await Task.WhenAll(tasks);
            
            for (int i = 0; i < ports.Length; i++)
            {
                if (results[i])
                {
                    availablePorts.Add(ports[i]);
                }
            }
            
            return availablePorts;
        }

        private async Task<bool> CheckPortAsync(int port, int timeoutMs)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
                
                var response = await httpClient.GetAsync($"http://localhost:{port}/rmsrestdataserver/rest");
                
                // RM pode retornar 401 (Unauthorized) que ainda indica que está funcionando
                return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Unauthorized;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Port {port} check failed: {ex.Message}");
                return false;
            }
        }
    }
}
