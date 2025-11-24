using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Pages.Servicios
{
    [Authorize(Roles = "Administrador,Admin")]
    public class EditModel : PageModel
    {
        private readonly IServicioRepository _serviciosRepo;
        private readonly ICategoriaServicioRepository _categoriasRepo;

        public EditModel(IServicioRepository serviciosRepo, ICategoriaServicioRepository categoriasRepo)
        {
            _serviciosRepo = serviciosRepo;
            _categoriasRepo = categoriasRepo;
        }

        public List<CategoriaServicio> Categorias { get; set; } = new();

        [BindProperty]
        public Servicio Servicio { get; set; } = new Servicio();

        public async Task<IActionResult> OnGetModalAsync(int id)
        {
            var s = await _serviciosRepo.GetByIdAsync(id);
            if (s == null) return NotFound();
            Servicio = s;
            Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
            return Partial("_EditPartial", this);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Servicio.Nombre = (Servicio.Nombre ?? string.Empty).Trim();
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_EditPartial", this);
            }

            if (Servicio.Precio <= 0)
            {
                ModelState.AddModelError("Servicio.Precio", "El precio debe ser mayor a 0");
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_EditPartial", this);
            }

            if (await _serviciosRepo.ExistsByNameAsync(Servicio.Nombre, Servicio.IdServicio))
            {
                ModelState.AddModelError("Servicio.Nombre", "Ya existe un servicio con ese nombre");
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_EditPartial", this);
            }

            if (!Servicio.IdCategoriaServicio.HasValue || Servicio.IdCategoriaServicio.Value <= 0)
            {
                ModelState.AddModelError("Servicio.IdCategoriaServicio", "Debe seleccionar una categoría");
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_EditPartial", this);
            }

            var existsCat = await _categoriasRepo.ExistsAsync(Servicio.IdCategoriaServicio.Value);
            if (!existsCat)
            {
                ModelState.AddModelError("Servicio.IdCategoriaServicio", "Categoría no válida");
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_EditPartial", this);
            }

            var original = await _serviciosRepo.GetByIdAsync(Servicio.IdServicio);
            if (original == null)
            {
                ModelState.AddModelError(string.Empty, "Servicio no encontrado");
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_EditPartial", this);
            }

            var sinCambios =
                original.Nombre == Servicio.Nombre &&
                original.Precio == Servicio.Precio &&
                original.DuracionEstimada == Servicio.DuracionEstimada &&
                original.IdCategoriaServicio == Servicio.IdCategoriaServicio &&
                original.Activo == Servicio.Activo;

            if (sinCambios)
            {
                return new JsonResult(new { success = false, unchanged = true, message = "Sin modificaciones" });
            }

            await _serviciosRepo.UpdateAsync(Servicio);
            return new JsonResult(new { success = true, message = "Servicio actualizado" });
        }
    }
}