using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Pages.Servicios
{
    [Authorize(Roles = "Administrador,Admin")]
    public class MovimientosDeleteModel : PageModel
    {
        private readonly IDetalleServicioRepository _detallesRepo;

        public MovimientosDeleteModel(IDetalleServicioRepository detallesRepo)
        {
            _detallesRepo = detallesRepo;
        }

        [BindProperty]
        public DetalleServicio Detalle { get; set; } = new DetalleServicio();

        public async Task<IActionResult> OnGetModalAsync(int id)
        {
            Detalle = await _detallesRepo.GetByIdAsync(id) ?? new DetalleServicio();
            if (Detalle.IdDetalleServicio == 0) return NotFound();
            return Partial("_MovimientosDeletePartial", this);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var id = Detalle.IdDetalleServicio;
            var serviceId = Detalle.IdServicio;
            await _detallesRepo.DeleteAsync(id);
            return new JsonResult(new { success = true, message = "Movimiento eliminado", serviceId });
        }
    }
}