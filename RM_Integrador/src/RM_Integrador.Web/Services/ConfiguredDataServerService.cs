using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using ConsumoDS.Models;
using Microsoft.Extensions.Configuration;

namespace RM_Integrador.Web.Services
{
    public class ConfiguredDataServerService
    {
        private readonly HttpClient _client;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly Dictionary<string, string> _customHeaders;

        public ConfiguredDataServerService(
            HttpClient httpClient,
            string baseUrl, 
            string username, 
            string password, 
            Dictionary<string, string>? customHeaders = null)
        {
            _client = httpClient;
            _baseUrl = baseUrl.TrimEnd('/');
            _username = username;
            _password = password;
            _customHeaders = customHeaders ?? new Dictionary<string, string>();

            ConfigureClient();
        }

        private void ConfigureClient()
        {
            try
            {
                // Limpa headers anteriores
                _client.DefaultRequestHeaders.Clear();

                // Configura autenticação básica
                var authToken = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes($"{_username}:{_password}"));
                _client.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

                Console.WriteLine($"Configurando cliente HTTP para URL base: {_baseUrl}");
                Console.WriteLine($"Autenticação configurada para usuário: {_username}");
                
                // Adiciona headers customizados
                foreach (var header in _customHeaders)
                {
                    _client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    Console.WriteLine($"Header adicionado: {header.Key}={header.Value}");
                }

                // Accept header
                _client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                Console.WriteLine("Header Accept: application/json adicionado");
                
                // Configura timeout
                _client.Timeout = TimeSpan.FromMinutes(5);
                Console.WriteLine($"Timeout do cliente HTTP configurado para: {_client.Timeout}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao configurar cliente HTTP: {ex.Message}");
                throw new InvalidOperationException("Erro ao configurar cliente HTTP", ex);
            }
        }

        public async Task<bool> AuthenticateAsync(string username, string password)
        {
            try
            {
                // O RM usa autenticação básica nos headers, não endpoint de login
                // Vamos testar com uma requisição simples
                var testUrl = $"{_baseUrl}/GetDataServers";
                var response = await _client.GetAsync(testUrl);
                
                // Se der 401, é problema de autenticação
                // Se der 404, é endpoint que não existe (normal)
                // Se der 500, pode ser problema do servidor
                // Vamos aceitar qualquer coisa que não seja 401 (Unauthorized)
                return response.StatusCode != System.Net.HttpStatusCode.Unauthorized;
            }
            catch (Exception)
            {
                // Em caso de erro, vamos tentar mesmo assim
                return true;
            }
        }

        public async Task<string> ExecuteGetAsync(string dataServerName, string? filter)
        {
            var url = string.IsNullOrEmpty(filter) 
                ? $"{_baseUrl}/{dataServerName}" 
                : $"{_baseUrl}/{dataServerName}/{filter}";

            Console.WriteLine($"Executando GET em: {url}");
            var response = await _client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Erro na requisição GET: {response.StatusCode} - {content}");
            }

            return content;
        }

        public async Task<string> ExecutePostAsync(string dataServerName, string jsonData)
        {
            try
            {
                if (string.IsNullOrEmpty(dataServerName))
                {
                    throw new ArgumentException("Nome do DataServer não pode ser vazio", nameof(dataServerName));
                }

                if (string.IsNullOrEmpty(jsonData))
                {
                    throw new ArgumentException("JSON não pode ser vazio", nameof(jsonData));
                }

                // Verifica se é um JSON válido
                try
                {
                    JsonDocument.Parse(jsonData);
                }
                catch (JsonException jex)
                {
                    throw new JsonException($"JSON inválido: {jex.Message}", jex);
                }

                var url = $"{_baseUrl}/{dataServerName}";
                Console.WriteLine($"Executando POST em: {url}");
                Console.WriteLine($"Dados: {jsonData}");
                Console.WriteLine($"Headers: {string.Join(", ", _client.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(",", h.Value)}"))}");

                // Verificar URL
                if (string.IsNullOrEmpty(_baseUrl))
                {
                    throw new InvalidOperationException("URL base não configurada");
                }

                // Verificar credenciais
                if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
                {
                    throw new InvalidOperationException("Credenciais não configuradas");
                }

                // Criar conteúdo da requisição
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                
                // Cria um timeout mais longo para o POST
                var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMinutes(5)); // 5 minutos de timeout
                
                // Executa o POST com timeout
                var response = await _client.PostAsync(url, content, cts.Token);
                
                // Lê a resposta
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Status code: {response.StatusCode}");
                Console.WriteLine($"Response: {responseContent}");

                // Verifica se foi bem-sucedido
                if (!response.IsSuccessStatusCode)
                {
                    // Verifica se é erro de CORS
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                        responseContent.Contains("CORS") || responseContent.Contains("cross-origin"))
                    {
                        throw new HttpRequestException($"Erro de CORS: O servidor recusou a requisição. StatusCode: {response.StatusCode}");
                    }
                    
                    // Outros erros HTTP
                    throw new HttpRequestException($"Erro na requisição POST: {response.StatusCode} - {responseContent}");
                }

                return responseContent;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro HTTP detalhado: {ex}");
                throw;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("A requisição POST foi cancelada por timeout");
                throw new TimeoutException("A requisição excedeu o tempo limite de 5 minutos");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("A requisição POST foi cancelada");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado no POST: {ex}");
                throw;
            }
        }
    }
}
