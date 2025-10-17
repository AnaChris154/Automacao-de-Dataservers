using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RM_Integrador.Web.Models;
using RM_Integrador.Web.Data; // Adicione esta linha
using RM_Integrador.Shared.Models;
using ConsumoDS.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RM_Integrador.Web.Services;

namespace RM_Integrador.Web.Controllers
{
    public class BibliotecaController : Controller
    {
        private readonly IDataServerService _dataServerService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BibliotecaController> _logger;
        private readonly IConfiguration _configuration; // Adicione isso

        public BibliotecaController(
            IDataServerService dataServerService, 
            ApplicationDbContext context,
            ILogger<BibliotecaController> logger,
            IConfiguration configuration) // Adicione isso
        {
            _dataServerService = dataServerService;
            _context = context;
            _logger = logger;
            _configuration = configuration; // Adicione isso
        }

        [HttpGet]
        public IActionResult Atualizar()
        {
            return View(new AtualizarBibliotecaViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Atualizar([FromForm]AtualizarBibliotecaViewModel model)
        {
            try
            {
                // Log detalhado dos dados recebidos
                _logger.LogInformation($@"Dados recebidos:
                    Name: {model.Name}
                    Routine: {model.Routine}
                    Description: {model.Description}
                    Keywords: {model.Keywords}
                    PrimaryKeys: {model.PrimaryKeys}
                    GetExample: {model.GetExample?.Substring(0, Math.Min(100, model.GetExample?.Length ?? 0))}...
                    PostExample: {model.PostExample}
                    WantToAddDocumentation: {model.WantToAddDocumentation}");

                if (model.WantToAddDocumentation)
                {
                    _logger.LogInformation($@"Dados de documentação:
                        CommonErrors: {model.CommonErrors}
                        ConsumptionTips: {model.ConsumptionTips}
                        FilterTips: {model.FilterTips}
                        UsageExamples: {model.UsageExamples}
                        Notes: {model.Notes}");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return Json(new { 
                        success = false, 
                        errors = errors 
                    });
                }

                var dataServer = await _context.DataServers
                    .FirstOrDefaultAsync(d => d.Name == model.Name);

                if (dataServer == null)
                {
                    dataServer = new DataServerInfo
                    {
                        Name = model.Name ?? string.Empty,
                        Routine = model.Routine ?? string.Empty,
                        Description = model.Description ?? string.Empty,
                        Keywords = !string.IsNullOrEmpty(model.Keywords) 
                            ? model.Keywords.Split(',').Select(k => k.Trim()).ToList() 
                            : new List<string>(),
                        PrimaryKeys = !string.IsNullOrEmpty(model.PrimaryKeys)
                            ? model.PrimaryKeys.Split(',').Select(k => k.Trim()).ToList()
                            : new List<string>(),
                        GetExample = model.GetExample ?? string.Empty,
                        PostExample = model.PostExample ?? string.Empty,
                        RequiresFilter = !string.IsNullOrWhiteSpace(model.PrimaryKeys),
                        
                        // Adicionar campos de documentação se o usuário optou por isso
                        CommonErrors = model.WantToAddDocumentation ? model.CommonErrors : null,
                        ConsumptionTips = model.WantToAddDocumentation ? model.ConsumptionTips : null,
                        FilterTips = model.WantToAddDocumentation ? model.FilterTips : null,
                        UsageExamples = model.WantToAddDocumentation ? model.UsageExamples : null,
                        Notes = model.WantToAddDocumentation ? model.Notes : null,
                        LastDocumentationUpdate = model.WantToAddDocumentation ? DateTime.Now : null,
                        DocumentationAuthor = model.WantToAddDocumentation ? User?.Identity?.Name : null
                    };

                    _context.DataServers.Add(dataServer);
                }
                else
                {
                    dataServer.Routine = model.Routine ?? dataServer.Routine;
                    dataServer.Description = model.Description ?? dataServer.Description;
                    dataServer.Keywords = !string.IsNullOrEmpty(model.Keywords)
                        ? model.Keywords.Split(',').Select(k => k.Trim()).ToList()
                        : dataServer.Keywords;
                    dataServer.PrimaryKeys = !string.IsNullOrEmpty(model.PrimaryKeys)
                        ? model.PrimaryKeys.Split(',').Select(k => k.Trim()).ToList()
                        : dataServer.PrimaryKeys;
                    dataServer.GetExample = model.GetExample ?? dataServer.GetExample;
                    dataServer.PostExample = model.PostExample ?? dataServer.PostExample;
                    dataServer.RequiresFilter = !string.IsNullOrWhiteSpace(model.PrimaryKeys);

                    // Atualizar campos de documentação se o usuário optou por isso
                    if (model.WantToAddDocumentation)
                    {
                        _logger.LogInformation("Atualizando campos de documentação...");
                        dataServer.CommonErrors = model.CommonErrors;
                        dataServer.ConsumptionTips = model.ConsumptionTips;
                        dataServer.FilterTips = model.FilterTips;
                        dataServer.UsageExamples = model.UsageExamples;
                        dataServer.Notes = model.Notes;
                        dataServer.LastDocumentationUpdate = DateTime.Now;
                        dataServer.DocumentationAuthor = User?.Identity?.Name;
                        _logger.LogInformation($"Documentação atualizada com sucesso. Autor: {User?.Identity?.Name}");
                    }
                    else
                    {
                        _logger.LogInformation("Usuário optou por não adicionar documentação, mantendo valores existentes");
                    }

                    _context.DataServers.Update(dataServer);
                }

                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Biblioteca atualizada com sucesso!" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao salvar DataServer: {ex.Message}");
                return Json(new { 
                    success = false, 
                    error = $"Erro ao salvar: {ex.Message}",
                    modelState = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    )
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarExemploGet(string dataServerName, string filter)
        {
            try
            {
                // Obter credenciais do RM
                var username = _configuration["RMSettings:Username"];
                var password = _configuration["RMSettings:Password"];
                var baseUrl = _configuration["RMSettings:BaseUrl"];

                // Configurar headers customizados
                var customHeaders = new Dictionary<string, string>();
                var codColigada = _configuration["RMSettings:CODCOLIGADA"];
                if (!string.IsNullOrEmpty(codColigada))
                {
                    customHeaders.Add("CODCOLIGADA", codColigada);
                }

                // Criar nova instância do serviço com as credenciais
                var rmService = new ConsumoDS.Services.DataServerService(
                    baseUrl,
                    username,
                    password,
                    customHeaders
                );

                // Autenticar antes de fazer a requisição
                if (!await rmService.AuthenticateAsync(username, password))
                {
                    return Json(new { 
                        success = false, 
                        error = "Falha na autenticação com o RM" 
                    });
                }

                // Fazer a requisição GET
                var result = await rmService.ExecuteGetAsync(dataServerName, filter);
                
                // Deserializa o resultado
                var jsonResult = JsonSerializer.Deserialize<DataServerResponse>(result);
                
                // Pega apenas o primeiro registro se existir
                var firstRecord = jsonResult?.Data?.FirstOrDefault(); // Corrigido de data para Data
                
                if (firstRecord == null)
                {
                    return Json(new { 
                        success = false, 
                        error = "Nenhum registro encontrado" 
                    });
                }

                // Formata o JSON do primeiro registro
                var formattedJson = JsonSerializer.Serialize(
                    firstRecord, 
                    new JsonSerializerOptions { 
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }
                );

                return Json(new { 
                    success = true, 
                    json = formattedJson 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao buscar exemplo GET: {ex.Message}");
                return Json(new { 
                    success = false, 
                    error = $"Erro ao buscar dados: {ex.Message}" 
                });
            }
        }
    }
}