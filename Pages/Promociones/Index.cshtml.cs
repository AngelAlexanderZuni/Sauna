using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Promociones
{
    [Authorize(Roles = "Administrador,Admin")]
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly IPromocionService _service;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IPromocionService service, ILogger<IndexModel> logger)
        {
            _service = service;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string? Term { get; set; }

        public List<PromocionDto> Promociones { get; set; } = new();
        public List<TipoDescuentoDto> Tipos { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                Promociones = (await _service.ListarAsync(Term)).ToList();
                Tipos = (await _service.TiposAsync()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar promociones");
                TempData["Error"] = "Error al cargar las promociones";
            }
        }

        public async Task<IActionResult> OnPostCrearAsync(PromocionCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Datos inválidos";
                    return RedirectToPage();
                }

                await _service.CrearAsync(dto);
                TempData["Success"] = "Promoción creada exitosamente";
                return RedirectToPage(new { term = Term });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear promoción");
                TempData["Error"] = ex.Message;
                return RedirectToPage();
            }
        }

        // NUEVO MÉTODO PARA AJAX
        public async Task<IActionResult> OnPostCrearAjaxAsync([FromForm] PromocionCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return new JsonResult(new { success = false, errors });
                }

                var nuevaPromocion = await _service.CrearAsync(dto);
                
                // Obtener el nombre del tipo para la respuesta
                var tipos = await _service.TiposAsync();
                var tipoNombre = tipos.FirstOrDefault(t => t.IdTipoDescuento == nuevaPromocion.IdTipoDescuento)?.Nombre ?? "";

                return new JsonResult(new
                {
                    success = true,
                    promo = new
                    {
                        idPromocion = nuevaPromocion.IdPromocion,
                        nombreDescuento = nuevaPromocion.NombreDescuento,
                        montoDescuento = nuevaPromocion.MontoDescuento,
                        idTipoDescuento = nuevaPromocion.IdTipoDescuento,
                        tipoNombre = tipoNombre,
                        valorCondicion = nuevaPromocion.ValorCondicion,
                        activo = nuevaPromocion.Activo,
                        motivo = nuevaPromocion.Motivo
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear promoción");
                return new JsonResult(new { success = false, errors = new[] { ex.Message } });
            }
        }

        public async Task<IActionResult> OnPostActualizarAsync(int id, PromocionCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Datos inválidos";
                    return RedirectToPage();
                }

                var updated = await _service.ActualizarAsync(id, dto);
                if (updated == null)
                {
                    TempData["Error"] = "Promoción no encontrada";
                    return RedirectToPage();
                }

                TempData["Success"] = "Promoción actualizada exitosamente";
                return RedirectToPage(new { term = Term });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar promoción");
                TempData["Error"] = ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                await _service.EliminarAsync(id);
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar promoción");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostCrearTipoAsync([FromBody] TipoRequest request)
        {
            try
            {
                var tipo = await _service.CrearTipoAsync(request.Nombre);
                return new JsonResult(new
                {
                    success = true,
                    tipo = new
                    {
                        idTipoDescuento = tipo.IdTipoDescuento,
                        nombre = tipo.Nombre
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear tipo");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostActualizarTipoAsync(int id, [FromBody] TipoRequest request)
        {
            try
            {
                var tipo = await _service.ActualizarTipoAsync(id, request.Nombre);
                if (tipo == null)
                    return new JsonResult(new { success = false, message = "Tipo no encontrado" });

                return new JsonResult(new
                {
                    success = true,
                    tipo = new
                    {
                        idTipoDescuento = tipo.IdTipoDescuento,
                        nombre = tipo.Nombre
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar tipo");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostEliminarTipoAsync(int id)
        {
            try
            {
                await _service.EliminarTipoAsync(id);
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar tipo");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public class TipoRequest
        {
            public string Nombre { get; set; } = string.Empty;
        }
    }
}