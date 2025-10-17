using Microsoft.AspNetCore.Mvc;
using RM_Integrador.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace RM_Integrador.Web.Controllers
{
    [Route("BancoDados")]
    public class BancoDadosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BancoDadosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Visualizar todos os dataservers
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var dataservers = await _context.DataServers
                .OrderBy(d => d.Routine)
                .ToListAsync();
            return View(dataservers);
        }

        // Exportar dataservers em CSV
        [HttpGet("Exportar")]
        public async Task<IActionResult> Exportar()
        {
            var dataservers = await _context.DataServers.ToListAsync(); // Mudado de DataServerInfos para DataServers

            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Routine,Description,PrimaryKeys,Keywords");

            foreach (var ds in dataservers)
            {
                sb.AppendLine($"\"{ds.Id}\",\"{ds.Name}\",\"{ds.Routine}\",\"{ds.Description}\",\"{string.Join(";", ds.PrimaryKeys ?? new List<string>())}\",\"{string.Join(";", ds.Keywords ?? new List<string>())}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "dataservers.csv");
        }

        [HttpGet("GetAllGrouped")]
        public async Task<IActionResult> GetAllGrouped()
        {
            try
            {
                var allDataServers = await _context.DataServers
                    .OrderBy(d => d.Name)
                    .Select(ds => new
                    {
                        ds.Name,
                        ds.Routine,
                        ds.Description,
                        PrimaryKeys = ds.PrimaryKeys ?? new List<string>(), // Garantir que não seja null
                        ds.Keywords
                    })
                    .ToListAsync();

                var groupedServers = allDataServers
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