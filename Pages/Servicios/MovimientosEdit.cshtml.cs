using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Pages.Servicios
{
    [Authorize(Roles = "Administrador,Admin")]
    public class MovimientosEditModel : PageModel
    {
        private readonly IDetalleServicioRepository _detallesRepo;

        public MovimientosEditModel(IDetalleServicioRepository detallesRepo)
        {
            _detallesRepo = detallesRepo;
        }

        [BindProperty]
        public DetalleServicio Detalle { get; set; } = new DetalleServicio();

        public async Task<IActionResult> OnGetModalAsync(int id)
        {
            Detalle = await _detallesRepo.GetByIdAsync(id) ?? new DetalleServicio();
            if (Detalle.IdDetalleServicio == 0) return NotFound();
            return Partial("_MovimientosEditPartial", this);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 422;
                return Partial("_MovimientosEditPartial", this);
            }

            var original = await _detallesRepo.GetByIdAsync(Detalle.IdDetalleServicio);
            if (original == null)
            {
                Response.StatusCode = 422;
                return Partial("_MovimientosEditPartial", this);
            }

            var sinCambios = original.Cantidad == Detalle.Cantidad && original.PrecioUnitario == Detalle.PrecioUnitario;
            if (sinCambios)
            {
                return new JsonResult(new { success = false, unchanged = true, message = "Sin modificaciones", serviceId = Detalle.IdServicio });
            }

            Detalle.Subtotal = Detalle.Cantidad * Detalle.PrecioUnitario;
            await _detallesRepo.UpdateAsync(Detalle);
            return new JsonResult(new { success = true, message = "Movimiento actualizado", serviceId = Detalle.IdServicio });
        }
    }
}