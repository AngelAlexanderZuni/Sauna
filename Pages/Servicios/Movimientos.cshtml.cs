using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Pages.Servicios
{
    [Authorize(Roles = "Administrador,Admin")]
    public class MovimientosModel : PageModel
    {
        private readonly IDetalleServicioRepository _detallesRepo;
        private readonly IServicioRepository _serviciosRepo;

        public MovimientosModel(IDetalleServicioRepository detallesRepo, IServicioRepository serviciosRepo)
        {
            _detallesRepo = detallesRepo;
            _serviciosRepo = serviciosRepo;
        }

        public Servicio Servicio { get; set; } = new Servicio();
        public List<DetalleServicio> Detalles { get; set; } = new List<DetalleServicio>();

        public async Task<IActionResult> OnGetModalAsync(int serviceId)
        {
            var s = await _serviciosRepo.GetByIdWithCategoriaAsync(serviceId);
            if (s == null) return NotFound();
            Servicio = s;
            Detalles = (await _detallesRepo.GetByServicioIdAsync(serviceId)).ToList();
            return Partial("_MovimientosPartial", this);
        }
    }
}