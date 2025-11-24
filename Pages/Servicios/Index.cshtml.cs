using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Pages.Servicios
{
    [Authorize(Roles = "Administrador,Admin")]
    public class IndexModel : PageModel
    {
        private readonly IServicioRepository _serviciosRepo;
        private readonly ICategoriaServicioRepository _categoriasRepo;

        public IndexModel(IServicioRepository serviciosRepo, ICategoriaServicioRepository categoriasRepo)
        {
            _serviciosRepo = serviciosRepo;
            _categoriasRepo = categoriasRepo;
        }

        public List<Servicio> Servicios { get; set; } = new();
        public List<CategoriaServicio> Categorias { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoriaId { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowInactive { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecioMin { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecioMax { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? DuracionMin { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? DuracionMax { get; set; }

        public async Task OnGetAsync()
        {
            Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
            Servicios = (await _serviciosRepo.SearchAsync(Busqueda, CategoriaId, ShowInactive, PrecioMin, PrecioMax, DuracionMin, DuracionMax)).ToList();
        }

        public async Task<IActionResult> OnGetListAsync()
        {
            Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList();
            Servicios = (await _serviciosRepo.SearchAsync(Busqueda, CategoriaId, ShowInactive, PrecioMin, PrecioMax, DuracionMin, DuracionMax)).ToList();
            return Partial("_TablePartial", this);
        }

        public async Task<IActionResult> OnPostCrearAsync(string nombre, decimal precio, int? duracionEstimada, int? categoriaId)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["Error"] = "El nombre es obligatorio";
                await OnGetAsync();
                return Page();
            }

            var servicio = new Servicio
            {
                Nombre = nombre.Trim(),
                Precio = precio,
                DuracionEstimada = duracionEstimada,
                IdCategoriaServicio = categoriaId > 0 ? categoriaId : null,
                Activo = true
            };

            await _serviciosRepo.AddAsync(servicio);
            TempData["Success"] = "Servicio creado";
            return RedirectToPage(new { Busqueda, CategoriaId, ShowInactive });
        }

        public async Task<IActionResult> OnPostActualizarAsync(int id, string nombre, decimal precio, int? duracionEstimada, int? categoriaId)
        {
            var servicio = await _serviciosRepo.GetByIdAsync(id);
            if (servicio == null)
            {
                TempData["Error"] = "Servicio no encontrado";
                await OnGetAsync();
                return Page();
            }

            servicio.Nombre = nombre.Trim();
            servicio.Precio = precio;
            servicio.DuracionEstimada = duracionEstimada;
            servicio.IdCategoriaServicio = categoriaId > 0 ? categoriaId : null;

            await _serviciosRepo.UpdateAsync(servicio);
            TempData["Success"] = "Servicio actualizado";
            return RedirectToPage(new { Busqueda, CategoriaId, ShowInactive });
        }

        public async Task<IActionResult> OnPostToggleActivoAsync(int id)
        {
            var servicio = await _serviciosRepo.GetByIdAsync(id);
            if (servicio == null)
            {
                TempData["Error"] = "Servicio no encontrado";
                await OnGetAsync();
                return Page();
            }
            servicio.Activo = !servicio.Activo;
            await _serviciosRepo.UpdateAsync(servicio);
            TempData["Success"] = servicio.Activo ? "Servicio activado" : "Servicio desactivado";
            return RedirectToPage(new { Busqueda, CategoriaId, ShowInactive });
        }
    }
}