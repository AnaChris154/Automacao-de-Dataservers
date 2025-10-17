using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RM_Integrador.Web.Services;

namespace RM_Integrador.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataServerController : Controller
    {
        private readonly IDataServerSearchService _searchService;
        private readonly ILogger<DataServerController> _logger;

        public DataServerController(
            IDataServerSearchService searchService,
            ILogger<DataServerController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string term)
        {
            try
            {
                _logger.LogInformation($"Buscando por termo: {term}");
                
                var results = await _searchService.SearchExamplesAsync(term);
                
                _logger.LogInformation($"Resultados encontrados: {results?.Count() ?? 0}");

                return Ok(new { 
                    success = true, 
                    data = results 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar exemplos");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }
    }
}