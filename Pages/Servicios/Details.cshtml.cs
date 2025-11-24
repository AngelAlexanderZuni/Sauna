using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Pages.Servicios
{
    [Authorize(Roles = "Administrador,Admin")]
    public class DetailsModel : PageModel
    {
        private readonly IServicioRepository _serviciosRepo;

        public DetailsModel(IServicioRepository serviciosRepo)
        {
            _serviciosRepo = serviciosRepo;
        }

        public Servicio Servicio { get; set; } = new Servicio();

        public async Task<IActionResult> OnGetModalAsync(int id)
        {
            var s = await _serviciosRepo.GetByIdWithCategoriaAsync(id);
            if (s == null) return NotFound();
            Servicio = s;
            return Partial("_DetailsPartial", this);
        }
    }
}