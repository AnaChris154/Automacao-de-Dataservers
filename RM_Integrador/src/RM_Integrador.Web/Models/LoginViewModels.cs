using System.ComponentModel.DataAnnotations;

namespace RM_Integrador.Web.Models
{
    public class LoginViewModel
    {
        [Display(Name = "URL de Consumo")]
        public string BaseUrl { get; set; } = "http://localhost:8051"; // URL padrão

        [Required(ErrorMessage = "O usuário é obrigatório")]
        [Display(Name = "Usuário")]
        [MinLength(1, ErrorMessage = "Usuário inválido")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        [MinLength(1, ErrorMessage = "Senha inválida")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Lembrar credenciais")]
        public bool RememberMe { get; set; }

        [Display(Name = "Administrador")]
        public bool IsAdmin { get; set; }

        public bool IsUser => !IsAdmin;

        public string GetFormattedUrl()
        {
            var url = BaseUrl?.TrimEnd('/') ?? "http://localhost:8051";
            const string suffix = "/rmsrestdataserver/rest";
            
            return url.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) 
                ? url 
                : url + suffix;
        }

        // Removidas as validações complexas de URL
        public bool IsValidRmUrl() => true;

        [Required]
        public string __RequestVerificationToken { get; set; } = string.Empty;
    }
}