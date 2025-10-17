using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RM_Integrador.Web.Models;
using RM_Integrador.Web.Services;

namespace RM_Integrador.Web.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;
        private readonly IDataServerSearchService _searchService;
        private readonly IDataServerService _dataServerService;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger,
            IDataServerSearchService searchService,
            IDataServerService dataServerService)
        {
            _userManager = userManager;
            _logger = logger;
            _searchService = searchService;
            _dataServerService = dataServerService;
        }

        public IActionResult Menu()
        {
            var loginType = User.Claims.FirstOrDefault(c => c.Type == "LoginType")?.Value;

            if (loginType != "Admin")
            {
                _logger.LogWarning($"Tentativa de acesso não autorizado ao menu admin: {User.Identity.Name}");
                return RedirectToAction("Menu", "User");
            }

            _logger.LogInformation($"Acesso autorizado ao menu admin: {User.Identity.Name}");
            return View();
        }

        // Gerenciamento de Usuários
        public async Task<IActionResult> GerenciarUsuarios()
        {
            var loginType = User.Claims.FirstOrDefault(c => c.Type == "LoginType")?.Value;
            if (loginType != "Admin")
            {
                return RedirectToAction("Menu", "User");
            }

            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // Biblioteca de JSONs
        public IActionResult AtualizarBiblioteca()
        {
            return RedirectToAction("Atualizar", "Biblioteca");
        }

        // Banco de Dados JSONs
        public IActionResult BancoDados()
        {
            return RedirectToAction("Index", "BancoDados");
        }

        // APIs para Banco de Dados
        [HttpGet]
        public async Task<IActionResult> ExportarDataServers()
        {
            try
            {
                var dataServers = await _searchService.GetAllDataServersAsync();
                return File(
                    System.Text.Encoding.UTF8.GetBytes(
                        System.Text.Json.JsonSerializer.Serialize(dataServers)
                    ),
                    "application/json",
                    "dataservers.json"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao exportar DataServers: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VisualizarDataServers()
        {
            try
            {
                var dataServers = await _searchService.GetAllDataServersAsync();
                return Json(new { success = true, data = dataServers });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao visualizar DataServers: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}