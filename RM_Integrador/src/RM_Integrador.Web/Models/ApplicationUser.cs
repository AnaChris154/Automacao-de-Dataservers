using Microsoft.AspNetCore.Identity;

namespace RM_Integrador.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string BaseUrl { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}