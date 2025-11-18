using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ProyectoSaunaKalixto.Web.Pages
{
    [Authorize]
    public class TestUserModel : PageModel
    {
        public string UserInfo { get; set; } = string.Empty;
        public List<string> Claims { get; set; } = new List<string>();

        public void OnGet()
        {
            UserInfo = $"Usuario: {User.Identity?.Name ?? "No name"}\n" +
                      $"Está autenticado: {User.Identity?.IsAuthenticated}\n" +
                      $"Tipo de autenticación: {User.Identity?.AuthenticationType ?? "None"}";

            foreach (var claim in User.Claims)
            {
                Claims.Add($"{claim.Type}: {claim.Value}");
            }

            // Log para debugging
            Console.WriteLine("=== INFORMACIÓN DEL USUARIO ===");
            Console.WriteLine(UserInfo);
            Console.WriteLine("=== CLAIMS ===");
            foreach (var claim in Claims)
            {
                Console.WriteLine(claim);
            }
            Console.WriteLine("==============================");
        }
    }
}