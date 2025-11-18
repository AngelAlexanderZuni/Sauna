using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProyectoSaunaKalixto.Web.Pages.Reportes
{
    [Authorize(Roles = "Administrador,Admin")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
