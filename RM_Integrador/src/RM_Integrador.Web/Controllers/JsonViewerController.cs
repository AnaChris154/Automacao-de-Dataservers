using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RM_Integrador.Web.Models;
using RM_Integrador.Web.Services;

namespace RM_Integrador.Web.Controllers
{
    [Route("JsonViewer")]
    public class JsonViewerController : Controller
    {
        private readonly IDataServerSearchService _searchService;

        public JsonViewerController(IDataServerSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        private string FormatJson(string jsonString)
        {
            try
            {
                // Se a string já contém escape characters, primeiro faz unescape
                if (jsonString.Contains("\\n") || jsonString.Contains("\\\""))
                {
                    jsonString = System.Text.Json.JsonSerializer.Deserialize<string>(jsonString);
                }

                // Parse para objeto e serializa formatado
                var jsonObj = System.Text.Json.JsonSerializer.Deserialize<dynamic>(jsonString);
                return System.Text.Json.JsonSerializer.Serialize(jsonObj, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao formatar JSON: {ex.Message}");
                return jsonString; // Retorna original se falhar
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> Search(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new { success = false, error = "Termo de busca não informado" });

            try
            {
                var results = await _searchService.SearchDataServersAsync(term);

                // Formata os JSONs antes de enviar
                foreach (var result in results)
                {
                    if (result.GetExample != null)
                        result.GetExample = FormatJson(result.GetExample);
                    if (result.PostExample != null)
                        result.PostExample = FormatJson(result.PostExample);
                }

                return Json(new { success = true, data = results });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}