using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RM_Integrador.Web.Models;
using RM_Integrador.Web.Services;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Principal;

namespace RM_Integrador.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRMConnectionService _rmConnection;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataServerService _dataServerService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IRMConnectionService rmConnection,
            IHttpClientFactory httpClientFactory,
            IDataServerService dataServerService,
            ILogger<AccountController> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _rmConnection = rmConnection ?? throw new ArgumentNullException(nameof(rmConnection));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _dataServerService = dataServerService ?? throw new ArgumentNullException(nameof(dataServerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user?.IsAdmin == true)
                {
                    return RedirectToAction("Menu", "Admin");
                }
                return RedirectToAction("Menu", "User");
            }
            return View();
        }

        private async Task<bool> EnsureAdminStatusAsync(ApplicationUser user)
        {
            if (user.UserName.ToLower() == "mestre")
            {
                if (!user.IsAdmin)
                {
                    user.IsAdmin = true;
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation($"Atualizado status admin para usuário mestre: {user.UserName}");
                }
                return true;
            }
            return user.IsAdmin;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm]LoginViewModel model)
        {
            try 
            {
                _logger.LogInformation($"Tentativa de login - Username: {model.Username}, IsAdmin: {model.IsAdmin}");

                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dados inválidos" });
                }

                // Usa o GetFormattedUrl() para garantir a URL correta
                var formattedUrl = model.GetFormattedUrl();
                _logger.LogInformation($"URL formatada: {formattedUrl}");

                // Tenta autenticar no RM
                try
                {
                    var customHeaders = new Dictionary<string, string> { { "CODCOLIGADA", "1" } };
                    var rmService = new ConsumoDS.Services.DataServerService(
                        formattedUrl, // Usa a URL formatada
                        model.Username,
                        model.Password,
                        customHeaders
                    );

                    var response = await rmService.ExecuteGetAsync("FopFuncData", null);
                    
                    if (string.IsNullOrEmpty(response))
                    {
                        return Json(new { success = false, message = "Falha na autenticação RM" });
                    }

                    // Se chegou aqui, autenticação foi bem-sucedida
                    _logger.LogInformation($"Autenticação RM bem-sucedida para: {formattedUrl}");
                }
                catch (Exception rmEx)
                {
                    _logger.LogError($"Erro RM: {rmEx.Message}");
                    return Json(new { success = false, message = $"Erro RM: {rmEx.Message}" });
                }

                // Procura ou cria o usuário
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = model.Username,
                        BaseUrl = model.BaseUrl,
                        EmailConfirmed = true
                    };
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                        return Json(new { success = false, message = "Erro ao criar usuário" });
                }
                else
                {
                    user.BaseUrl = model.BaseUrl;
                    await _userManager.UpdateAsync(user);
                }

                // Cria claims com base na escolha do login
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim("BaseUrl", model.BaseUrl ?? ""),
                    new Claim("LoginType", model.IsAdmin ? "Admin" : "User")
                };

                // Adiciona as claims ao usuário
                var existingClaims = await _userManager.GetClaimsAsync(user);
                
                // Remove claims antigas se existirem
                var oldLoginTypeClaim = existingClaims.FirstOrDefault(c => c.Type == "LoginType");
                if (oldLoginTypeClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, oldLoginTypeClaim);
                }
                
                var oldBaseUrlClaim = existingClaims.FirstOrDefault(c => c.Type == "BaseUrl");
                if (oldBaseUrlClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, oldBaseUrlClaim);
                }

                // Adiciona as novas claims
                await _userManager.AddClaimsAsync(user, claims);

                // Cria um ClaimsIdentity personalizado com as claims
                var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Faz o sign in com o principal personalizado
                await _signInManager.SignInWithClaimsAsync(user, model.RememberMe, claims);

                // Redireciona conforme a escolha do login
                string redirectUrl = model.IsAdmin ? 
                    Url.Action("Menu", "Admin") : 
                    Url.Action("Menu", "User");

                return Json(new { success = true, redirectUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                _logger.LogInformation("Realizando logout");

                // Faz o sign out do Identity
                await _signInManager.SignOutAsync();

                // Limpa todos os cookies do site
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                // Limpa a sessão
               // HttpContext.Session.Clear();

                // Força o navegador a não cachear
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro no logout: {ex.Message}");
            }

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult RefreshSession()
        {
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> CheckAuthStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            return Json(new 
            { 
                isAuthenticated = User?.Identity?.IsAuthenticated ?? false,
                username = user?.UserName,
                isAdmin = user?.IsAdmin ?? false
            });
        }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RedirectUrl { get; set; }
    }

    public class DataServerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly Dictionary<string, string> _headers;

        public DataServerService(HttpClient httpClient, string baseUrl, string username, string password, Dictionary<string, string> headers)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
            _username = username;
            _password = password;
            _headers = headers;
        }

        // Use _httpClient para todas as requisições!
    }
}