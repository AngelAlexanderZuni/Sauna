using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Pages.Servicios
{
    [Authorize(Roles = "Administrador,Admin")]
    public class CreateModel : PageModel
    {
        private readonly IServicioRepository _serviciosRepo;
        private readonly ICategoriaServicioRepository _categoriasRepo;

        public CreateModel(IServicioRepository serviciosRepo, ICategoriaServicioRepository categoriasRepo)
        {
            _serviciosRepo = serviciosRepo;
            _categoriasRepo = categoriasRepo;
        }

        public List<CategoriaServicio> Categorias { get; set; } = new();

        [BindProperty]
        public Servicio Servicio { get; set; } = new Servicio();

        public async Task<IActionResult> OnGetModalAsync()
        {
            Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
            return Partial("_CreatePartial", this);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Servicio.Nombre = (Servicio.Nombre ?? string.Empty).Trim();
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_CreatePartial", this);
            }

            if (Servicio.Precio <= 0)
            {
                ModelState.AddModelError("Servicio.Precio", "El precio debe ser mayor a 0");
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_CreatePartial", this);
            }

            if (await _serviciosRepo.ExistsByNameAsync(Servicio.Nombre))
            {
                ModelState.AddModelError("Servicio.Nombre", "Ya existe un servicio con ese nombre");
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_CreatePartial", this);
            }

            if (!Servicio.IdCategoriaServicio.HasValue || Servicio.IdCategoriaServicio.Value <= 0)
            {
                ModelState.AddModelError("Servicio.IdCategoriaServicio", "Debe seleccionar una categoría");
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_CreatePartial", this);
            }

            var existsCat = await _categoriasRepo.ExistsAsync(Servicio.IdCategoriaServicio.Value);
            if (!existsCat)
            {
                ModelState.AddModelError("Servicio.IdCategoriaServicio", "Categoría no válida");
                Response.StatusCode = 422;
                Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
                return Partial("_CreatePartial", this);
            }

            await _serviciosRepo.AddAsync(Servicio);
            return new JsonResult(new { success = true, message = "Servicio creado" });
        }
    }
}