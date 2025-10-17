using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RM_Integrador.Web.Services;
using System.Text.Json;
using ConsumoDS.Models;

namespace RM_Integrador.Web.Controllers
{
    [Authorize]
    [Route("SearchDS")]
    public class SearchDSController : Controller
    {
        private readonly IDataServerSearchService _searchService;

        public SearchDSController(IDataServerSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("SearchByKeywords")]
        public async Task<IActionResult> SearchByKeywords(string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term))
                    return Json(new { success = false, error = "Termo de busca não informado" });

                var results = await _searchService.SearchByKeywordsAsync(term);
                return Json(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("GetAllGrouped")]
        public async Task<IActionResult> GetAllGrouped()
        {
            try
            {
                var allDataServers = await _searchService.GetAllDataServersAsync();

                var formattedDataServers = allDataServers.Select(ds => new
                {
                    ds.Id,
                    ds.Name,
                    ds.Routine,
                    ds.Description,
                    PrimaryKeys = ds.PrimaryKeys ?? new List<string>(), // Garantir que não seja null
                    ds.Keywords,
                    ds.CommonErrors,
                    ds.ConsumptionTips,
                    ds.FilterTips,
                    ds.UsageExamples,
                    ds.Notes
                });

                var groupedServers = formattedDataServers
                    .GroupBy(ds => ds.Name.Substring(0, 3).ToUpper())
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.OrderBy(ds => ds.Name).ToList());

                var systemNames = new Dictionary<string, string>
                {
                    { "FOP", "Folha de Pagamento" },
                    { "RHU", "Gestão de Pessoas" },
                    { "PTO", "Automação de Ponto" },
                    { "CTR", "Contratos" },
                    { "CMP", "Compras" },
                    { "FIN", "Financeiro" },
                    { "EST", "Estoque" },
                    { "FAT", "Faturamento" }
                };

                return Json(new { 
                    success = true, 
                    data = new {
                        groups = groupedServers,
                        systemNames = systemNames
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
