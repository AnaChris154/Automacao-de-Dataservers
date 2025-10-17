using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Adicione esta linha
using RM_Integrador.Web.Services;
using System.Text.Json;
using ConsumoDS.Models;
using Microsoft.Extensions.Logging;
using RM_Integrador.Web.Data;
using Microsoft.Extensions.Configuration;

namespace RM_Integrador.Web.Controllers
{
    public class CompareJsonController : Controller
    {
        private readonly IDataServerService _dataServerService;
        private readonly ILogger<CompareJsonController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public CompareJsonController(
            IDataServerService dataServerService,
            ILogger<CompareJsonController> logger,
            ApplicationDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _dataServerService = dataServerService;
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Método para debug - verificar dados do DataServer
        [HttpGet]
        public async Task<IActionResult> GetDataServerInfo(string dataServerName)
        {
            try
            {
                _logger.LogInformation($"Buscando informações do DataServer: {dataServerName}");

                var dataServer = await _context.DataServers
                    .FirstOrDefaultAsync(d => d.Name == dataServerName);

                if (dataServer == null)
                {
                    return Json(new { 
                        success = false, 
                        error = "DataServer não encontrado",
                        availableDataServers = await _context.DataServers
                            .Select(d => d.Name)
                            .Take(10)
                            .ToListAsync()
                    });
                }

                return Json(new
                {
                    success = true,
                    dataServer = new
                    {
                        Name = dataServer.Name,
                        Routine = dataServer.Routine,
                        PrimaryKeys = dataServer.PrimaryKeys,
                        HasPostExample = !string.IsNullOrEmpty(dataServer.PostExample),
                        PostExampleLength = dataServer.PostExample?.Length ?? 0,
                        PostExample = dataServer.PostExample // Para debug
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar DataServer: {ex}");
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompareJson([FromBody] CompareJsonRequest model)
        {
            try
            {
                _logger.LogInformation($"Iniciando comparação JSON para DataServer: {model.DataServerName}");
                
                // Validação inicial dos dados de entrada
                if (string.IsNullOrEmpty(model.DataServerName))
                {
                    return Json(new { 
                        success = false, 
                        error = "Nome do DataServer é obrigatório" 
                    });
                }

                if (string.IsNullOrEmpty(model.UserJson))
                {
                    return Json(new { 
                        success = false, 
                        error = "JSON do usuário é obrigatório" 
                    });
                }

                // Busca o exemplo do DataServer direto do banco
                var dataServer = await _context.DataServers
                    .FirstOrDefaultAsync(d => d.Name == model.DataServerName);

                _logger.LogInformation($"DataServer encontrado: {dataServer != null}");
                
                if (dataServer == null)
                {
                    return Json(new { 
                        success = false, 
                        error = $"DataServer '{model.DataServerName}' não encontrado na base de dados" 
                    });
                }

                if (string.IsNullOrEmpty(dataServer.PostExample))
                {
                    return Json(new { 
                        success = false, 
                        error = $"PostExample vazio para o DataServer '{model.DataServerName}'" 
                    });
                }

                _logger.LogInformation($"PostExample encontrado: {dataServer.PostExample.Length} caracteres");

                // Validação e parsing dos JSONs com tratamento de erro específico
                JsonElement userJson;
                JsonElement exampleJson;

                try
                {
                    userJson = JsonDocument.Parse(model.UserJson).RootElement;
                    _logger.LogInformation("JSON do usuário parseado com sucesso");
                }
                catch (JsonException ex)
                {
                    return Json(new { 
                        success = false, 
                        error = $"Erro no JSON do usuário: {ex.Message}" 
                    });
                }

                try
                {
                    exampleJson = JsonDocument.Parse(dataServer.PostExample).RootElement;
                    _logger.LogInformation("JSON de exemplo parseado com sucesso");
                }
                catch (JsonException ex)
                {
                    return Json(new { 
                        success = false, 
                        error = $"Erro no JSON de exemplo do DataServer: {ex.Message}" 
                    });
                }

                var differences = new List<JsonDifference>();

                // Criar dicionários case-insensitive para facilitar a comparação
                var userProps = userJson.EnumerateObject()
                    .ToDictionary(p => p.Name.ToUpper(), p => p.Value);
                var exampleProps = exampleJson.EnumerateObject()
                    .ToDictionary(p => p.Name.ToUpper(), p => p.Value);

                _logger.LogInformation($"Propriedades do usuário: {userProps.Count}, Propriedades do exemplo: {exampleProps.Count}");

                // 3. Compara campos obrigatórios
                foreach (var prop in exampleJson.EnumerateObject())
                {
                    var upperPropName = prop.Name.ToUpper();
                    if (!userProps.ContainsKey(upperPropName))
                    {
                        differences.Add(new JsonDifference
                        {
                            Field = prop.Name,
                            Type = DifferenceType.Missing,
                            Message = $"Campo '{prop.Name}' será adicionado"
                        });
                        continue;
                    }

                    var userValue = userProps[upperPropName];
                    if (userValue.ValueKind != JsonValueKind.Null && 
                        userValue.ValueKind != prop.Value.ValueKind)
                    {
                        differences.Add(new JsonDifference
                        {
                            Field = prop.Name,
                            Type = DifferenceType.TypeMismatch,
                            Message = $"Campo '{prop.Name}' tem tipo incorreto"
                        });
                    }
                }

                // 4. Verifica campos extras
                foreach (var prop in userJson.EnumerateObject())
                {
                    var upperPropName = prop.Name.ToUpper();
                    if (!exampleProps.ContainsKey(upperPropName))
                    {
                        if (prop.Value.ValueKind != JsonValueKind.Null)
                        {
                            differences.Add(new JsonDifference
                            {
                                Field = prop.Name,
                                Type = DifferenceType.Extra,
                                Message = $"Campo extra '{prop.Name}' encontrado"
                            });
                        }
                    }
                }

                // 5. Gera JSON corrigido
                var correctedJson = CreateCorrectedJson(exampleJson, userJson);

                // Se o JSON está correto, tenta fazer o POST
                if (model.ExecutePost)
                {
                    try
                    {
                        var baseUrl = _configuration["RMSettings:BaseUrl"];
                        _logger.LogInformation($"Usando URL do RM para POST: {baseUrl}");

                        var username = _configuration["RMSettings:Username"];
                        var password = _configuration["RMSettings:Password"];
                        var codColigada = _configuration["RMSettings:CODCOLIGADA"];

                        var customHeaders = new Dictionary<string, string>();
                        if (!string.IsNullOrEmpty(codColigada))
                        {
                            customHeaders.Add("CODCOLIGADA", codColigada);
                        }

                        var httpClient = _httpClientFactory.CreateClient("default");
                        var rmService = new RM_Integrador.Web.Services.ConfiguredDataServerService(
                            httpClient,
                            baseUrl,
                            username,
                            password,
                            customHeaders
                        );

                        _logger.LogInformation($"Executando POST para: {model.DataServerName}");
                        var postResponse = await rmService.ExecutePostAsync(
                            model.DataServerName,
                            correctedJson
                        );

                        return Json(new
                        {
                            success = true,
                            data = new
                            {
                                differences = differences,
                                exampleJson = FormatJson(dataServer.PostExample),
                                correctedJson = correctedJson,
                                postResponse = FormatJson(postResponse)
                            }
                        });
                    }
                    catch (Exception postEx)
                    {
                        _logger.LogError($"Erro ao executar POST: {postEx.Message}");
                        return Json(new
                        {
                            success = false,
                            error = $"Erro ao executar POST: {postEx.Message}"
                        });
                    }
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        differences = differences,
                        exampleJson = FormatJson(dataServer.PostExample),
                        correctedJson = correctedJson
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro geral ao comparar JSON: {ex}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return Json(new { 
                    success = false, 
                    error = $"Erro ao comparar JSON: {ex.Message}",
                    details = ex.ToString() // Para debug - remover em produção
                });
            }
        }

        private string CreateCorrectedJson(JsonElement example, JsonElement userJson)
        {
            try
            {
                var corrected = new Dictionary<string, JsonElement>();
                
                // Criar dicionários case-insensitive
                var userProps = userJson.EnumerateObject()
                    .ToDictionary(p => p.Name.ToUpper(), p => p.Value);
                var exampleProps = example.EnumerateObject()
                    .ToDictionary(p => p.Name.ToUpper(), p => p.Value);

                // 1. Primeiro adiciona todos os campos do exemplo (para manter o case correto)
                foreach (var prop in example.EnumerateObject())
                {
                    var upperPropName = prop.Name.ToUpper();
                    if (userProps.ContainsKey(upperPropName) && 
                        userProps[upperPropName].ValueKind != JsonValueKind.Null)
                    {
                        // Usa o valor do usuário mas mantém o nome do campo do exemplo
                        corrected[prop.Name] = userProps[upperPropName];
                    }
                    else if (prop.Value.ValueKind != JsonValueKind.Null)
                    {
                        // Usa o valor do exemplo
                        corrected[prop.Name] = prop.Value;
                    }
                }

                // 2. Adiciona campos extras do usuário que não existem no exemplo
                foreach (var prop in userJson.EnumerateObject())
                {
                    var upperPropName = prop.Name.ToUpper();
                    if (!exampleProps.ContainsKey(upperPropName) && 
                        prop.Value.ValueKind != JsonValueKind.Null)
                    {
                        corrected[prop.Name] = prop.Value;
                    }
                }

                return JsonSerializer.Serialize(corrected, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar JSON corrigido: {ex.Message}");
                throw;
            }
        }

        private string FormatJson(string json)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
            }
            catch
            {
                return json;
            }
        }
    }

    public class CompareJsonRequest
    {
        public string DataServerName { get; set; }
        public string UserJson { get; set; }
        public bool ExecutePost { get; set; } // Adicione esta propriedade
    }

    public class JsonDifference
    {
        public string Field { get; set; }
        public DifferenceType Type { get; set; }
        public string Message { get; set; }
    }

    public enum DifferenceType
    {
        Missing,
        TypeMismatch,
        Extra,
        NullValue
    }
}