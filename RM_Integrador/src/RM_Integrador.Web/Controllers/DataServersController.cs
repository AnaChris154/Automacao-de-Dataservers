using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RM_Integrador.Web.Data;
using RM_Integrador.Shared.Models;

namespace RM_Integrador.Web.Controllers
{
    [Authorize] // Adiciona proteção a todas as actions
    public class DataServersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataServersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DataServers
        public async Task<IActionResult> Index()
        {
            return View(await _context.DataServers.ToListAsync()); // Alterado aqui
        }

        // GET: DataServers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dataServer = await _context.DataServers // E aqui
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dataServer == null)
            {
                return NotFound();
            }

            return View(dataServer);
        }
    }
}