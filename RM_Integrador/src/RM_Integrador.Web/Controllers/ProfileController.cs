using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore; // Adicione esta linha
using RM_Integrador.Web.Models;

namespace RM_Integrador.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var loginType = User.Claims.FirstOrDefault(c => c.Type == "LoginType")?.Value;
            ViewBag.LoginType = loginType;
            ViewBag.UserName = user.UserName;
            ViewBag.Email = user.Email;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out do Identity
            await _signInManager.SignOutAsync();
            
            // Limpa todos os cookies do site
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // Limpa a sessão
            HttpContext.Session.Clear();

            // Redireciona para o login com cache control
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult EmDesenvolvimento()
        {
            TempData["Aviso"] = "Funcionalidade em desenvolvimento!";
            return RedirectToAction("Index");
        }

        // Adicione este método para listar usuários
        [Authorize]
        public async Task<IActionResult> GerenciarUsuarios()
        {
            var loginType = User.Claims.FirstOrDefault(c => c.Type == "LoginType")?.Value;
            if (loginType != "Admin")
            {
                return RedirectToAction("Menu", "User");
            }

            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.BaseUrl,
                    u.IsAdmin,
                    u.EmailConfirmed
                })
                .ToListAsync();

            return View(users);
        }
    }
}