using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RM_Integrador.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProxyController> _logger;

        public ProxyController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("forward-rm-request")]
        public async Task<IActionResult> ForwardRMRequest([FromBody] ProxyRequestModel model)
        {
            try
            {
                _logger.LogInformation($"Recebida solicitação para encaminhar para RM: {model.Url}");
                
                // Criar HttpClient com timeout mais longo
                var httpClient = _httpClientFactory.CreateClient("default");
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                
                // Configurar autenticação básica
                var username = _configuration["RMSettings:Username"];
                var password = _configuration["RMSettings:Password"];
                var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                
                // Adicionar headers CODCOLIGADA
                var codColigada = _configuration["RMSettings:CODCOLIGADA"];
                if (!string.IsNullOrEmpty(codColigada))
                {
                    httpClient.DefaultRequestHeaders.Add("CODCOLIGADA", codColigada);
                }
                
                // Adicionar Accept header
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                // Montar URL completa
                var baseUrl = _configuration["RMSettings:BaseUrl"];
                var url = $"{baseUrl.TrimEnd('/')}/{model.DataServerName}";
                
                _logger.LogInformation($"URL final: {url}");
                _logger.LogInformation($"Método: {model.Method}");
                
                HttpResponseMessage response;
                
                if (model.Method.ToUpper() == "GET")
                {
                    // Se tiver filtro, adicionar à URL
                    if (!string.IsNullOrEmpty(model.Filter))
                    {
                        url = $"{url}/{model.Filter}";
                    }
                    
                    _logger.LogInformation($"Executando GET em: {url}");
                    response = await httpClient.GetAsync(url);
                }
                else if (model.Method.ToUpper() == "POST")
                {
                    // Verificar se o JSON é válido
                    if (string.IsNullOrEmpty(model.JsonData))
                    {
                        return BadRequest(new { success = false, error = "Dados JSON não fornecidos para POST" });
                    }
                    
                    try
                    {
                        // Validar se é um JSON válido
                        JsonDocument.Parse(model.JsonData);
                    }
                    catch (JsonException jex)
                    {
                        return BadRequest(new { success = false, error = $"JSON inválido: {jex.Message}" });
                    }
                    
                    _logger.LogInformation($"Executando POST em: {url}");
                    _logger.LogInformation($"Dados: {model.JsonData}");
                    
                    var content = new StringContent(model.JsonData, Encoding.UTF8, "application/json");
                    response = await httpClient.PostAsync(url, content);
                }
                else
                {
                    return BadRequest(new { success = false, error = $"Método HTTP não suportado: {model.Method}" });
                }
                
                // Ler resposta
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Status code: {response.StatusCode}");
                _logger.LogInformation($"Resposta: {responseContent.Substring(0, Math.Min(responseContent.Length, 500))}...");
                
                // Verificar se foi bem-sucedido
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { 
                        success = false, 
                        statusCode = (int)response.StatusCode,
                        error = $"Erro na requisição: {response.StatusCode} - {responseContent}" 
                    });
                }
                
                // Tentar deserializar como JSON antes de retornar
                try
                {
                    var jsonResult = JsonDocument.Parse(responseContent);
                    return Ok(new { success = true, data = responseContent });
                }
                catch
                {
                    // Se não for JSON válido, retornar como texto
                    return Ok(new { success = true, data = responseContent });
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Erro HTTP ao encaminhar requisição: {ex.Message}");
                return StatusCode(500, new { 
                    success = false, 
                    error = $"Erro de comunicação com o RM: {ex.Message}",
                    details = ex.InnerException?.Message
                });
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("A requisição foi cancelada por timeout");
                return StatusCode(504, new { 
                    success = false, 
                    error = "A requisição excedeu o tempo limite de 5 minutos" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro não esperado ao encaminhar requisição: {ex}");
                return StatusCode(500, new { 
                    success = false, 
                    error = $"Erro ao processar requisição: {ex.Message}" 
                });
            }
        }
    }

    public class ProxyRequestModel
    {
        public string Method { get; set; } = "GET";
        public string DataServerName { get; set; }
        public string Filter { get; set; }
        public string JsonData { get; set; }
        public string Url { get; set; }
    }
}
