using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;
using System.Security.Claims;
using System.Text.Json;

namespace ProyectoSaunaKalixto.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Admin")]
    public class EgresosApiController : ControllerBase
    {
        private readonly EgresoService _egresoService;
        private readonly FileUploadService _fileUploadService;
        private readonly ILogger<EgresosApiController> _logger;

        public EgresosApiController(
            EgresoService egresoService, 
            FileUploadService fileUploadService,
            ILogger<EgresosApiController> logger)
        {
            _egresoService = egresoService;
            _fileUploadService = fileUploadService;
            _logger = logger;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CrearEgreso([FromForm] string data)
        {
            _logger.LogInformation("📥 API: Recibiendo petición POST con archivos");

            try
            {
                // Deserializar el JSON que viene en el campo 'data'
                var dto = JsonSerializer.Deserialize<EgresoCreateDTO>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dto == null)
                {
                    return BadRequest(new { success = false, message = "Datos inválidos" });
                }

                _logger.LogInformation("📄 DTO deserializado: {DetallesCount} detalles", dto.Detalles?.Count ?? 0);

                // Procesar archivos si existen
                var files = Request.Form.Files;
                _logger.LogInformation("📎 Archivos recibidos: {Count}", files.Count);

                if (files.Count > 0 && dto.Detalles != null)
                {
                    for (int i = 0; i < dto.Detalles.Count && i < files.Count; i++)
                    {
                        var file = files[$"archivo_{i}"];
                        if (file != null && file.Length > 0)
                        {
                            try
                            {
                                var rutaArchivo = await _fileUploadService.SaveFileAsync(file, "egresos");
                                dto.Detalles[i].ComprobanteRuta = rutaArchivo;
                                _logger.LogInformation("✅ Archivo guardado para detalle {Index}: {Ruta}", i, rutaArchivo);
                            }
                            catch (ArgumentException ex)
                            {
                                _logger.LogWarning("⚠️ Error al guardar archivo {Index}: {Error}", i, ex.Message);
                                // Continuar sin el archivo si hay error
                            }
                        }
                    }
                }

                // Obtener usuario
                var userIdClaim = User.FindFirstValue("UserId");
                if (!int.TryParse(userIdClaim, out int idUsuario) || idUsuario <= 0)
                {
                    var nameIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!int.TryParse(nameIdClaim, out idUsuario) || idUsuario <= 0)
                    {
                        return Unauthorized(new { success = false, message = "Usuario no identificado" });
                    }
                }

                var resultado = await _egresoService.CrearEgresoAsync(dto, idUsuario);
                
                _logger.LogInformation("✅ Egreso creado: ID={Id}, Total=S/ {Total:N2}", 
                    resultado.IdCabEgreso, resultado.MontoTotal);

                return Ok(new
                {
                    success = true,
                    message = $"Egreso registrado correctamente. Total: S/ {resultado.MontoTotal:N2}",
                    data = resultado
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar JSON");
                return BadRequest(new { success = false, message = "Error en el formato de datos" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Error de validación");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error de base de datos: " + (dbEx.InnerException?.Message ?? dbEx.Message)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error interno: " + ex.Message 
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarEgreso(int id, [FromForm] string data)
        {
            _logger.LogInformation("📥 API: Recibiendo petición PUT para egreso {Id} con archivos", id);

            try
            {
                var dto = JsonSerializer.Deserialize<EgresoCreateDTO>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dto == null)
                {
                    return BadRequest(new { success = false, message = "Datos inválidos" });
                }

                // Obtener egreso actual para manejar archivos existentes
                var egresoActual = await _egresoService.ObtenerPorIdAsync(id);
                
                // Procesar archivos nuevos
                var files = Request.Form.Files;
                _logger.LogInformation("📎 Archivos nuevos recibidos: {Count}", files.Count);

                if (dto.Detalles != null)
                {
                    for (int i = 0; i < dto.Detalles.Count; i++)
                    {
                        var file = files[$"archivo_{i}"];
                        
                        if (file != null && file.Length > 0)
                        {
                            // Hay archivo nuevo
                            try
                            {
                                // Eliminar archivo anterior si existe y es un archivo
                                if (egresoActual != null && i < egresoActual.Detalles.Count)
                                {
                                    var rutaAnterior = egresoActual.Detalles[i].ComprobanteRuta;
                                    if (!string.IsNullOrEmpty(rutaAnterior) && FileUploadService.IsFilePath(rutaAnterior))
                                    {
                                        await _fileUploadService.DeleteFileAsync(rutaAnterior);
                                        _logger.LogInformation("🗑️ Archivo anterior eliminado: {Ruta}", rutaAnterior);
                                    }
                                }

                                var rutaArchivo = await _fileUploadService.SaveFileAsync(file, "egresos");
                                dto.Detalles[i].ComprobanteRuta = rutaArchivo;
                                _logger.LogInformation("✅ Archivo nuevo guardado: {Ruta}", rutaArchivo);
                            }
                            catch (ArgumentException ex)
                            {
                                _logger.LogWarning("⚠️ Error al guardar archivo: {Error}", ex.Message);
                            }
                        }
                        // Si no hay archivo nuevo pero el usuario envió comprobanteRuta como texto, se mantiene
                    }
                }

                // Obtener usuario
                var userIdClaim = User.FindFirstValue("UserId");
                if (!int.TryParse(userIdClaim, out int idUsuario) || idUsuario <= 0)
                {
                    var nameIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!int.TryParse(nameIdClaim, out idUsuario) || idUsuario <= 0)
                    {
                        return Unauthorized(new { success = false, message = "Usuario no identificado" });
                    }
                }

                var resultado = await _egresoService.ActualizarEgresoAsync(id, dto, idUsuario);
                
                _logger.LogInformation("✅ Egreso actualizado: ID {Id}, Total S/ {Total:N2}", 
                    resultado.IdCabEgreso, resultado.MontoTotal);
                
                return Ok(new 
                { 
                    success = true, 
                    message = $"Egreso actualizado correctamente. Total: S/ {resultado.MontoTotal:N2}",
                    data = resultado
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al deserializar JSON");
                return BadRequest(new { success = false, message = "Error en el formato de datos" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Validación fallida: {Message}", ex.Message);
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error de base de datos");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error de base de datos: " + (dbEx.InnerException?.Message ?? dbEx.Message)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado");
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error interno: " + ex.Message 
                });
            }
        }
    }
}
