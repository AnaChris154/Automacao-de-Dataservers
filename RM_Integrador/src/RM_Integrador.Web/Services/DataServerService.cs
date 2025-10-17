using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using RM_Integrador.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RM_Integrador.Web.Models;

namespace RM_Integrador.Web.Services
{
    public interface IDataServerService
    {
        void ConfigureConnection(string baseUrl, string username, string password);
        Task<bool> AuthenticateAsync(string username, string password);
        Task<object> GetDataServerInfoAsync(string dataServerName);
        Task<object> ExecuteGetAsync(string dataServerName, Dictionary<string, string> parameters);
        Task<object> ExecutePostAsync(string dataServerName, object data);
    }

    public class DataServerService : IDataServerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private string _baseUrl;
        private string _username;
        private string _password;
        private Dictionary<string, string> _headers;
        private readonly IConfiguration _configuration;
        private readonly RMSettings _rmSettings;

        public DataServerService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IOptions<RMSettings> rmSettings)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _rmSettings = rmSettings.Value;
            
            // Inicializar com valores padrão das configurações
            _baseUrl = _rmSettings.BaseUrl?.TrimEnd('/') ?? string.Empty;
            _username = _rmSettings.Username ?? string.Empty;
            _password = _rmSettings.Password ?? string.Empty;
            _headers = new Dictionary<string, string>();
            
            // Configurar headers padrão se necessário
            if (!string.IsNullOrEmpty(_rmSettings.CODCOLIGADA))
            {
                _headers["CODCOLIGADA"] = _rmSettings.CODCOLIGADA;
            }
        }

        public void ConfigureConnection(string baseUrl, string username, string password)
        {
            _baseUrl = baseUrl?.TrimEnd('/');
            _username = username;
            _password = password;
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            using var client = _httpClientFactory.CreateClient();
            // Implementar lógica de autenticação real aqui
            return await Task.FromResult(true);
        }

        public async Task<object> GetDataServerInfoAsync(string dataServerName)
        {
            try
            {
                Console.WriteLine($"Buscando DataServer: {dataServerName}");
                
                // Validação do parâmetro
                if (string.IsNullOrEmpty(dataServerName))
                {
                    Console.WriteLine("DataServerName está vazio ou nulo");
                    return null;
                }
                
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                Console.WriteLine($"Connection string configurada: {!string.IsNullOrEmpty(connectionString)}");
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT Name, Routine, PrimaryKeys, PostExample
                    FROM DataServers 
                    WHERE Name = @Name";

                Console.WriteLine($"Query: {query}");
                Console.WriteLine($"Parâmetro @Name: '{dataServerName}'");

                using var command = new SqlCommand(query, connection);
                
                // Parametrização mais explícita
                var parameter = new SqlParameter("@Name", SqlDbType.NVarChar, 4000);
                parameter.Value = dataServerName;
                command.Parameters.Add(parameter);
                
                Console.WriteLine($"Comando criado com {command.Parameters.Count} parâmetros");
                Console.WriteLine($"Parâmetro: {command.Parameters[0].ParameterName} = '{command.Parameters[0].Value}'");

                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    var dataServer = new DataServerInfo
                    {
                        Name = reader.GetString(0),
                        Routine = reader.GetString(1),
                        PrimaryKeys = JsonSerializer.Deserialize<List<string>>(reader.GetString(2)),
                        PostExample = reader.GetString(3)
                    };

                    Console.WriteLine($"DataServer encontrado: {JsonSerializer.Serialize(dataServer)}");
                    return dataServer;
                }

                Console.WriteLine("DataServer não encontrado no banco");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar DataServer: {ex.Message}");
                throw;
            }
        }

        public async Task<object> ExecuteGetAsync(string dataServerName, Dictionary<string, string> parameters)
        {
            using var client = _httpClientFactory.CreateClient();
            
            // Configurar autenticação básica
            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
            }
            
            // Adicionar header de CODCOLIGADA se disponível
            if (_headers != null && _headers.ContainsKey("CODCOLIGADA"))
            {
                client.DefaultRequestHeaders.Add("CODCOLIGADA", _headers["CODCOLIGADA"]);
            }
            
            // Constrói a URL base - se dataServerName já contém o ID, não precisa encodificar a parte do ID
            var url = $"{_baseUrl}/{dataServerName}";
            
            // Se contém caracteres especiais como $ que são válidos na URL do RM, 
            // não vamos encodificar esses caracteres específicos
            if (dataServerName.Contains("$") || dataServerName.Contains("_"))
            {
                // Para IDs do RM, apenas substitui espaços por %20 se houver
                url = url.Replace(" ", "%20");
                Console.WriteLine($"[DEBUG] URL com ID específico (sem encoding completo): {url}");
            }
            
            // Add parameters to query string
            if (parameters?.Any() == true)
            {
                var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                url = $"{url}?{queryString}";
            }

            Console.WriteLine($"Executando GET para: {url}");
            Console.WriteLine($"Headers: Authorization = Basic *****, CODCOLIGADA = {_headers?.GetValueOrDefault("CODCOLIGADA", "não definido")}");

            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Status da resposta: {response.StatusCode}");
            Console.WriteLine($"Conteúdo da resposta: {content}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Erro na requisição: {response.StatusCode} - {content}");
            }
            
            return JsonSerializer.Deserialize<object>(content) ?? new object();
        }

        public async Task<object> ExecutePostAsync(string dataServerName, object data)
        {
            using var client = _httpClientFactory.CreateClient();
            
            // Configurar autenticação básica
            if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
            {
                var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
            }
            
            // Adicionar header de CODCOLIGADA se disponível
            if (_headers != null && _headers.ContainsKey("CODCOLIGADA"))
            {
                client.DefaultRequestHeaders.Add("CODCOLIGADA", _headers["CODCOLIGADA"]);
            }
            
            var url = $"{_baseUrl}/{dataServerName}";
            
            // Log detalhado do que será enviado
            Console.WriteLine($"=== DETALHES DA REQUISIÇÃO POST ===");
            Console.WriteLine($"URL: {url}");
            Console.WriteLine($"Tipo do objeto de dados: {data?.GetType().FullName ?? "null"}");
            
            string json;
            if (data is string dataString)
            {
                // Se já é uma string, assume que é JSON válido
                json = dataString;
                Console.WriteLine($"Dados recebidos como STRING: {json}");
            }
            else
            {
                // Serializa o objeto para JSON
                json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = false });
                Console.WriteLine($"Objeto serializado para JSON: {json}");
            }
            
            Console.WriteLine($"JSON Length: {json.Length}");
            Console.WriteLine($"JSON bem formatado:");
            try
            {
                var formatted = JsonSerializer.Serialize(JsonSerializer.Deserialize<object>(json), new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(formatted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao formatar JSON: {ex.Message}");
            }
            
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            Console.WriteLine($"Content-Type: application/json; charset=utf-8");
            Console.WriteLine($"Authorization: Basic [HIDDEN]");
            Console.WriteLine($"CODCOLIGADA: {_headers?.GetValueOrDefault("CODCOLIGADA", "não definido")}");
            Console.WriteLine($"=== FIM DOS DETALHES ===");
            
            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Status da resposta: {response.StatusCode}");
            Console.WriteLine($"Conteúdo da resposta: {responseContent}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Erro na requisição: {response.StatusCode} - {responseContent}");
            }
            
            return JsonSerializer.Deserialize<object>(responseContent) ?? new object();
        }
    }
}         