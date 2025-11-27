using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Services;
using System.Security.Claims;
using System.Text.Json;

namespace ProyectoSaunaKalixto.Web.Pages.Egresos
{
    [Authorize(Roles = "Administrador,Admin")]
    public class IndexModel : PageModel
    {
        private readonly EgresoService _egresoService;
        private readonly FileUploadService _fileUploadService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            EgresoService egresoService, 
            FileUploadService fileUploadService,
            ILogger<IndexModel> logger)
        {
            _egresoService = egresoService;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        // ===== PROPIEDADES =====
        
        public List<EgresoDTO> Egresos { get; set; } = new();
        public List<TipoEgreso> TiposEgreso { get; set; } = new();
        public decimal TotalEgresos { get; set; }
        
        // Para pasar al modal de edición
        public EgresoDTO? EgresoEnEdicion { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaInicio { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaFin { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? IdTipoEgresoFiltro { get; set; }

        // ===== GET HANDLERS =====
        
        public async Task OnGetAsync()
        {
            FechaInicio ??= DateTime.Now.AddMonths(-1);
            FechaFin ??= DateTime.Now;

            Egresos = await _egresoService.ObtenerPorRangoFechasAsync(FechaInicio.Value, FechaFin.Value);
            TiposEgreso = await _egresoService.ObtenerTiposEgresoAsync();

            // Filtrar por tipo si se especificó
            if (IdTipoEgresoFiltro.HasValue)
            {
                Egresos = Egresos.Where(e => 
                    e.Detalles.Any(d => d.IdTipoEgreso == IdTipoEgresoFiltro.Value)
                ).ToList();
            }

            TotalEgresos = Egresos.Sum(e => e.MontoTotal);

            _logger.LogInformation("📊 Egresos cargados: {Count} registros, Total: S/ {Total:N2}", 
                Egresos.Count, TotalEgresos);
        }

        // ===== MODAL CREATE =====
        
        public async Task<IActionResult> OnGetCreateModalAsync()
        {
            // Cargar tipos de egreso para el modal
            TiposEgreso = await _egresoService.ObtenerTiposEgresoAsync();
            _logger.LogInformation("📝 Modal crear egreso abierto. Tipos disponibles: {Count}", TiposEgreso.Count);
            
            // Retornar el partial con este modelo (IndexModel)
            return Partial("_CreatePartial", this);
        }

        public async Task<IActionResult> OnPostCreateAsync([FromBody] EgresoCreateDTO dto)
        {
            try
            {
                var userIdClaim = User.FindFirstValue("UserId");
                if (!int.TryParse(userIdClaim, out int idUsuario))
                {
                    return new JsonResult(new { success = false, message = "Usuario no identificado" }) 
                    { StatusCode = 401 };
                }

                var resultado = await _egresoService.CrearEgresoAsync(dto, idUsuario);
                
                return new JsonResult(new 
                { 
                    success = true, 
                    message = $"Egreso registrado correctamente. Total: S/ {resultado.MontoTotal:N2}",
                    data = resultado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear egreso");
                return new JsonResult(new { success = false, message = ex.Message }) 
                { StatusCode = 400 };
            }
        }

        // ===== MODAL DETAILS =====
        
        public async Task<IActionResult> OnGetDetailsModalAsync(int id)
        {
            var egreso = await _egresoService.ObtenerPorIdAsync(id);
            if (egreso == null)
                return NotFound();

            return Partial("_DetailsPartial", egreso);
        }

        // ===== MODAL EDIT =====
        
        public async Task<IActionResult> OnGetEditModalAsync(int id)
        {
            _logger.LogInformation("🔍 Intentando cargar egreso con ID: {Id}", id);
            
            var egreso = await _egresoService.ObtenerPorIdAsync(id);
            
            if (egreso == null)
            {
                _logger.LogWarning("⚠️ No se encontró egreso con ID: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("✅ Egreso encontrado: {Id}, Detalles: {Count}", egreso.IdCabEgreso, egreso.Detalles?.Count ?? 0);

            // Cargar tipos de egreso para el modal
            TiposEgreso = await _egresoService.ObtenerTiposEgresoAsync();
            
            // Pasar el egreso como propiedad del modelo
            EgresoEnEdicion = egreso;
            _logger.LogInformation("📝 Modal editar egreso {Id} preparado con {DetallesCount} detalles", id, egreso.Detalles?.Count ?? 0);
            
            // Retornar el partial con este modelo (IndexModel)
            return Partial("_EditPartial", this);
        }

        // ===== MODAL DELETE =====
        
        public async Task<IActionResult> OnGetDeleteModalAsync(int id)
        {
            var egreso = await _egresoService.ObtenerPorIdAsync(id);
            if (egreso == null)
                return NotFound();

            return Partial("_DeletePartial", egreso);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var resultado = await _egresoService.EliminarAsync(id);
                
                if (resultado)
                {
                    return new JsonResult(new 
                    { 
                        success = true, 
                        message = "Egreso eliminado correctamente" 
                    });
                }
                
                return new JsonResult(new 
                { 
                    success = false, 
                    message = "No se encontró el egreso" 
                }) 
                { StatusCode = 404 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar egreso");
                return new JsonResult(new { success = false, message = ex.Message }) 
                { StatusCode = 500 };
            }
        }

        // ===== DOWNLOAD FILE =====

        public IActionResult OnGetDownloadFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    return NotFound();
                }

                var absolutePath = _fileUploadService.GetAbsolutePath(filePath);
                
                if (string.IsNullOrEmpty(absolutePath) || !System.IO.File.Exists(absolutePath))
                {
                    _logger.LogWarning("Archivo no encontrado: {Path}", filePath);
                    return NotFound();
                }

                var fileName = Path.GetFileName(absolutePath);
                var contentType = GetContentType(absolutePath);

                return PhysicalFile(absolutePath, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al descargar archivo");
                return StatusCode(500);
            }
        }

        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        // ===== TIPOS DE EGRESO HANDLERS =====

        public async Task<IActionResult> OnGetTiposModalAsync()
        {
            try
            {
                TiposEgreso = await _egresoService.ObtenerTiposEgresoAsync();
                return Partial("_TiposModal", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar modal de tipos de egreso");
                return BadRequest(new { success = false, message = "Error al cargar tipos de egreso" });
            }
        }

        public async Task<JsonResult> OnGetListarTiposAsync()
        {
            try
            {
                var tipos = await _egresoService.ObtenerTiposEgresoAsync();
                return new JsonResult(new { success = true, data = tipos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar tipos de egreso");
                return new JsonResult(new { success = false, message = "Error al listar tipos" });
            }
        }

        public async Task<JsonResult> OnPostCrearTipoAsync(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return new JsonResult(new { success = false, message = "Nombre requerido" });
                }

                // Validar nombre único
                var tipos = await _egresoService.ObtenerTiposEgresoAsync();
                if (tipos.Any(t => t.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    return new JsonResult(new { success = false, message = "Ya existe un tipo con ese nombre" });
                }

                var nuevoTipo = new TipoEgreso { Nombre = nombre };
                await _egresoService.CrearTipoEgresoAsync(nuevoTipo);
                
                _logger.LogInformation("✅ Tipo de egreso creado: {Nombre}", nombre);
                return new JsonResult(new { success = true, message = "Tipo creado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear tipo de egreso");
                return new JsonResult(new { success = false, message = "Error al crear tipo" });
            }
        }

        public async Task<JsonResult> OnPostActualizarTipoAsync(int id, string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return new JsonResult(new { success = false, message = "Nombre requerido" });
                }
                
                // Validar nombre único (excepto el mismo tipo)
                var tipos = await _egresoService.ObtenerTiposEgresoAsync();
                if (tipos.Any(t => t.IdTipoEgreso != id && t.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    return new JsonResult(new { success = false, message = "Ya existe un tipo con ese nombre" });
                }

                await _egresoService.ActualizarTipoEgresoAsync(id, nombre);
                
                _logger.LogInformation("✅ Tipo de egreso actualizado: ID {Id}, Nombre: {Nombre}", id, nombre);
                return new JsonResult(new { success = true, message = "Tipo actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar tipo de egreso");
                return new JsonResult(new { success = false, message = "Error al actualizar tipo" });
            }
        }

        public async Task<JsonResult> OnPostEliminarTipoAsync(int id)
        {
            try
            {
                // Verificar si tiene egresos asociados
                var tieneEgresos = await _egresoService.TipoTieneEgresosAsync(id);
                if (tieneEgresos)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "No se puede eliminar. Este tipo tiene egresos asociados." 
                    });
                }

                await _egresoService.EliminarTipoEgresoAsync(id);
                
                _logger.LogInformation("🗑️ Tipo de egreso eliminado: ID {Id}", id);
                return new JsonResult(new { success = true, message = "Tipo eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar tipo de egreso");
                return new JsonResult(new { success = false, message = "Error al eliminar tipo" });
            }
        }

        // ===== DTOs para TipoEgreso =====
        public class TipoEgresoCreateDTO
        {
            public string Nombre { get; set; } = string.Empty;
        }

        public class TipoEgresoUpdateDTO
        {
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
