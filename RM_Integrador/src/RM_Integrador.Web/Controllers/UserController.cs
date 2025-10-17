using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RM_Integrador.Web.Services;

namespace RM_Integrador.Web.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IDataServerSearchService _searchService;
        private readonly IDataServerService _dataServerService;

        public UserController(
            ILogger<UserController> logger,
            IDataServerSearchService searchService,
            IDataServerService dataServerService)
        {
            _logger = logger;
            _searchService = searchService;
            _dataServerService = dataServerService;
        }

        public IActionResult Menu()
        {
            var loginType = User.Claims.FirstOrDefault(c => c.Type == "LoginType")?.Value;
            _logger.LogInformation($"Acesso ao menu de usuário - LoginType: {loginType}");
            return View();
        }
        
        // Endpoint para carregar conteúdo em abas
        [HttpGet]
        public IActionResult GetTabContent(string page)
        {
            try
            {
                return page?.ToLower() switch
                {
                    "testrequests" => RedirectToAction("TabContent", "TestRequests"),
                    "jsonviewer" => RedirectToAction("Index", "JsonViewer"),
                    "comparejson" => RedirectToAction("Index", "CompareJson"),
                    "searchds" => RedirectToAction("Index", "SearchDS"),
                    "dataservers" => RedirectToAction("Index", "DataServers"),
                    _ => BadRequest("Página não encontrada")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar conteúdo da aba: {Page}", page);
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        [HttpGet]
        public async Task<IActionResult> BuscarJsonsExemplos(string dataServerName)
        {
            try
            {
                var result = await _searchService.SearchExamplesAsync(dataServerName);
                if (!result.Any())
                {
                    return Json(new { success = false, message = "Nenhum DataServer encontrado" });
                }
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TesteGET(string dataServerName, string? filter)
        {
            try
            {
                _logger.LogInformation($"[DEBUG] TesteGET - DataServer: {dataServerName}, Filter: {filter}");
                
                if (string.IsNullOrWhiteSpace(dataServerName))
                {
                    _logger.LogWarning("[DEBUG] Nome do DataServer não fornecido");
                    return Json(new { success = false, message = "Nome do DataServer é obrigatório" });
                }

                // Se o filtro parece ser um ID (contém caracteres especiais como $, _, etc), 
                // adiciona à URL em vez de usar como parâmetro
                string targetDataServer = dataServerName;
                var parameters = new Dictionary<string, string>();
                
                if (!string.IsNullOrEmpty(filter))
                {
                    // Verifica se é um ID específico (contém caracteres como $, _, números)
                    if (IsSpecificId(filter))
                    {
                        targetDataServer = $"{dataServerName}/{filter}";
                        _logger.LogInformation($"[DEBUG] Usando filtro como ID na URL: {targetDataServer}");
                    }
                    else
                    {
                        // Usa como parâmetro de query string
                        parameters.Add("filter", filter);
                        _logger.LogInformation($"[DEBUG] Usando filtro como parâmetro: {filter}");
                    }
                }

                var result = await _dataServerService.ExecuteGetAsync(targetDataServer, parameters);
                
                _logger.LogInformation($"[DEBUG] Resultado do GET - Success: dados retornados");
                
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[DEBUG] Erro no TesteGET: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private bool IsSpecificId(string filter)
        {
            // Considera como ID específico se contém caracteres típicos de IDs do RM
            // como $, _, ou se parece com um código (números + caracteres especiais)
            return filter.Contains("$") || 
                   filter.Contains("_") ||
                   (filter.Any(char.IsDigit) && filter.Any(c => !char.IsLetterOrDigit(c))) ||
                   System.Text.RegularExpressions.Regex.IsMatch(filter, @"^\d+[\$_]\w*");
        }

        [HttpPost]
        public async Task<IActionResult> TestePOST(string dataServerName, string executionMode = "remote")
        {
            try
            {
                // Lê o body raw da requisição
                using var reader = new StreamReader(Request.Body);
                var jsonData = await reader.ReadToEndAsync();
                
                _logger.LogInformation($"[DEBUG] TestePOST - DataServer: {dataServerName}, JSON Length: {jsonData?.Length ?? 0}");
                _logger.LogInformation($"[DEBUG] JSON Raw Data: {jsonData}");
                
                if (string.IsNullOrWhiteSpace(dataServerName))
                {
                    _logger.LogWarning("[DEBUG] Nome do DataServer não fornecido no POST");
                    return Json(new { success = false, message = "Nome do DataServer é obrigatório" });
                }

                if (string.IsNullOrWhiteSpace(jsonData))
                {
                    _logger.LogWarning("[DEBUG] Dados JSON não fornecidos no POST");
                    return Json(new { success = false, message = "Dados JSON são obrigatórios" });
                }

                // Valida se é um JSON válido e converte para objeto
                object dataObject;
                try
                {
                    dataObject = System.Text.Json.JsonSerializer.Deserialize<object>(jsonData);
                    _logger.LogInformation($"[DEBUG] JSON é válido: {System.Text.Json.JsonSerializer.Serialize(dataObject)}");
                }
                catch (Exception parseEx)
                {
                    _logger.LogError($"[DEBUG] JSON inválido: {parseEx.Message}");
                    return Json(new { success = false, message = "JSON inválido: " + parseEx.Message });
                }

                _logger.LogInformation($"[DEBUG] Enviando OBJETO (não string) para DataServerService.ExecutePostAsync...");
                
                // Passa o objeto deserializado, não a string JSON
                var result = await _dataServerService.ExecutePostAsync(dataServerName, dataObject);
                
                _logger.LogInformation($"[DEBUG] Resultado do POST - Success: dados retornados");
                
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[DEBUG] Erro no TestePOST: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}