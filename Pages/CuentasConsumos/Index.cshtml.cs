using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.CuentasConsumos
{
    [Authorize(Roles = "Administrador,Admin,Recepcionista")]
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        private readonly ICuentaService _cuentaService;
        private readonly IDetalleServicioService _detalleServicioService;
        private readonly IDetalleConsumoService _detalleConsumoService;
        private readonly IClienteService _clienteService;
        private readonly IServicioService _servicioService;
        private readonly IProductoService _productoService;
        private readonly IPromocionService _promocionService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ICuentaService cuentaService,
            IDetalleServicioService detalleServicioService,
            IDetalleConsumoService detalleConsumoService,
            IClienteService clienteService,
            IServicioService servicioService,
            IProductoService productoService,
            IPromocionService promocionService,
            ILogger<IndexModel> logger)
        {
            _cuentaService = cuentaService;
            _detalleServicioService = detalleServicioService;
            _detalleConsumoService = detalleConsumoService;
            _clienteService = clienteService;
            _servicioService = servicioService;
            _productoService = productoService;
            _promocionService = promocionService;
            _logger = logger;
        }

        public List<CuentaDto> Cuentas { get; set; } = new();
        public List<ClienteDTO> Clientes { get; set; } = new();
        public List<ServicioDto> Servicios { get; set; } = new();
        public List<ProductoDto> Productos { get; set; } = new();
        public List<PromocionDto> Promociones { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public bool SoloAbiertas { get; set; } = true;

        public async Task OnGetAsync()
        {
            try
            {
                Cuentas = (await _cuentaService.ListarAsync(SoloAbiertas)).ToList();
                Clientes = (await _clienteService.ListarAsync(null)).ToList();
                Servicios = (await _servicioService.ListarAsync(null)).ToList();
                Productos = (await _productoService.ListarAsync(null)).ToList();
                Promociones = (await _promocionService.ListarAsync(null)).Where(p => p.Activo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos de cuentas");
                TempData["Error"] = "Error al cargar los datos";
            }
        }

        // ============================================
        // CREAR CUENTA
        // ============================================
        public async Task<IActionResult> OnPostCrearCuentaAjaxAsync([FromForm] CuentaCreateDto dto)
        {
            try
            {
                _logger.LogInformation("=== Iniciando creación de cuenta ===");
                _logger.LogInformation("IdCliente recibido: {IdCliente}", dto.IdCliente);
                _logger.LogInformation("IdPromocion recibido: {IdPromocion}", dto.IdPromocion);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    _logger.LogWarning("ModelState inválido: {Errors}", string.Join(", ", errors));
                    return new JsonResult(new { success = false, errors });
                }

                if (dto.IdCliente <= 0)
                {
                    return new JsonResult(new { success = false, errors = new[] { "Debe seleccionar un cliente válido" } });
                }

                dto.IdEstadoCuenta = 1;
                dto.IdUsuarioCreador = 0;
                
                _logger.LogInformation("Creando cuenta con IdUsuarioCreador: {UserId}", dto.IdUsuarioCreador);

                var nuevaCuenta = await _cuentaService.CrearAsync(dto);
                
                _logger.LogInformation("✅ Cuenta creada exitosamente: {CuentaId}", nuevaCuenta.IdCuenta);

                        return new JsonResult(new
                        {
                            success = true,
                            cuenta = new
                            {
                                idCuenta = nuevaCuenta.IdCuenta,
                                fechaHoraCreacion = nuevaCuenta.FechaHoraCreacion.ToString("dd/MM/yyyy HH:mm"),
                                clienteNombre = nuevaCuenta.ClienteNombre ?? "N/A",
                                subtotalServicios = nuevaCuenta.SubtotalServicios,
                                subtotalProductos = nuevaCuenta.SubtotalProductos,
                                subtotalConsumos = nuevaCuenta.SubtotalConsumos,
                                descuento = nuevaCuenta.Descuento,
                                total = nuevaCuenta.Total,
                                estadoNombre = nuevaCuenta.EstadoNombre ?? "Abierta",
                                promocionNombre = nuevaCuenta.PromocionNombre,
                                promocionMonto = nuevaCuenta.PromocionMontoDescuento
                            }
                        });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al crear cuenta");
                _logger.LogError("Mensaje: {Message}", ex.Message);
                _logger.LogError("InnerException: {InnerMessage}", ex.InnerException?.Message ?? "No disponible");
                _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
                
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                
                return new JsonResult(new { 
                    success = false, 
                    errors = new[] { errorMessage }
                });
            }
        }

        // ============================================
        // CERRAR CUENTA (PAGAR)
        // ============================================
        public async Task<IActionResult> OnPostCerrarCuentaAsync(int id)
        {
            try
            {
                var cerrada = await _cuentaService.CerrarAsync(id, DateTime.Now);
                if (cerrada == null)
                    return new JsonResult(new { success = false, message = "Cuenta no encontrada" });

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar cuenta {Id}", id);
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // CANCELAR CUENTA
        // ============================================
        public async Task<IActionResult> OnPostCancelarCuentaAsync(int id)
        {
            var operationId = Guid.NewGuid();
            _logger.LogInformation("[{OperationId}] Cancelando cuenta: {CuentaId}", operationId, id);

            try
            {
                if (id <= 0)
                {
                    return new JsonResult(new { success = false, message = "ID de cuenta inválido" });
                }

                var servicios = await _detalleServicioService.ListarPorCuentaAsync(id);
                foreach (var servicio in servicios)
                {
                    await _detalleServicioService.EliminarAsync(servicio.IdDetalleServicio);
                }

                var productos = await _detalleConsumoService.ListarPorCuentaAsync(id);
                foreach (var producto in productos)
                {
                    await _detalleConsumoService.EliminarAsync(producto.IdDetalle);
                }

                await _cuentaService.EliminarAsync(id);

                _logger.LogInformation("[{OperationId}] ✅ Cuenta cancelada exitosamente: {CuentaId}", operationId, id);

                return new JsonResult(new { success = true, message = "Cuenta cancelada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{OperationId}] ❌ Error al cancelar cuenta: {CuentaId}", operationId, id);
                return new JsonResult(new { success = false, message = "Error al cancelar la cuenta: " + ex.Message });
            }
        }

        // ============================================
        // AGREGAR SERVICIO
        // ============================================
        public async Task<IActionResult> OnPostAgregarServicioAjaxAsync([FromForm] DetalleServicioCreateDto dto)
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

                var detalle = await _detalleServicioService.CrearAsync(dto);
                var servicio = await _servicioService.ObtenerAsync(detalle.IdServicio);
                var cuenta = await _cuentaService.ObtenerAsync(dto.IdCuenta);

                return new JsonResult(new
                {
                    success = true,
                    detalle = new
                    {
                        idDetalleServicio = detalle.IdDetalleServicio,
                        cantidad = detalle.Cantidad,
                        precioUnitario = detalle.PrecioUnitario,
                        subtotal = detalle.Subtotal,
                        servicioNombre = servicio?.Nombre ?? "N/A"
                    },
                    cuenta = new
                    {
                        subtotalServicios = cuenta?.SubtotalServicios ?? 0,
                        subtotalProductos = cuenta?.SubtotalProductos ?? 0,
                        subtotalConsumos = cuenta?.SubtotalConsumos ?? 0,
                        descuento = cuenta?.Descuento ?? 0,
                        total = cuenta?.Total ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar servicio");
                return new JsonResult(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ============================================
        // AGREGAR PRODUCTO
        // ============================================
        public async Task<IActionResult> OnPostAgregarProductoAjaxAsync([FromForm] DetalleConsumoCreateDto dto)
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

                var detalle = await _detalleConsumoService.CrearAsync(dto);
                var producto = await _productoService.ObtenerAsync(detalle.IdProducto);
                var cuenta = await _cuentaService.ObtenerAsync(dto.IdCuenta);

                return new JsonResult(new
                {
                    success = true,
                    detalle = new
                    {
                        idDetalle = detalle.IdDetalle,
                        cantidad = detalle.Cantidad,
                        precioUnitario = detalle.PrecioUnitario,
                        subtotal = detalle.Subtotal,
                        productoNombre = producto?.Nombre ?? "N/A"
                    },
                    cuenta = new
                    {
                        subtotalServicios = cuenta?.SubtotalServicios ?? 0,
                        subtotalProductos = cuenta?.SubtotalProductos ?? 0,
                        subtotalConsumos = cuenta?.SubtotalConsumos ?? 0,
                        descuento = cuenta?.Descuento ?? 0,
                        total = cuenta?.Total ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar producto");
                return new JsonResult(new { success = false, errors = new[] { ex.Message } });
            }
        }

        // ============================================
        // ELIMINAR SERVICIO
        // ============================================
        public async Task<IActionResult> OnPostEliminarServicioAsync(int id, int idCuenta)
        {
            try
            {
                await _detalleServicioService.EliminarAsync(id);
                var cuenta = await _cuentaService.ObtenerAsync(idCuenta);
                
                return new JsonResult(new 
                { 
                    success = true,
                    cuenta = new
                    {
                        subtotalServicios = cuenta?.SubtotalServicios ?? 0,
                        subtotalProductos = cuenta?.SubtotalProductos ?? 0,
                        subtotalConsumos = cuenta?.SubtotalConsumos ?? 0,
                        descuento = cuenta?.Descuento ?? 0,
                        total = cuenta?.Total ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar servicio");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // ELIMINAR PRODUCTO
        // ============================================
        public async Task<IActionResult> OnPostEliminarProductoAsync(int id, int idCuenta)
        {
            try
            {
                await _detalleConsumoService.EliminarAsync(id);
                var cuenta = await _cuentaService.ObtenerAsync(idCuenta);
                
                return new JsonResult(new 
                { 
                    success = true,
                    cuenta = new
                    {
                        subtotalServicios = cuenta?.SubtotalServicios ?? 0,
                        subtotalProductos = cuenta?.SubtotalProductos ?? 0,
                        subtotalConsumos = cuenta?.SubtotalConsumos ?? 0,
                        descuento = cuenta?.Descuento ?? 0,
                        total = cuenta?.Total ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // EDITAR CANTIDAD DE PRODUCTO
        // ============================================
        public async Task<IActionResult> OnPostEditarCantidadProductoAsync(int id, int cantidad, int idCuenta)
        {
            try
            {
                if (cantidad <= 0)
                {
                    return new JsonResult(new { success = false, message = "La cantidad debe ser mayor a 0" });
                }

                // Obtener todos los productos de la cuenta
                var productos = await _detalleConsumoService.ListarPorCuentaAsync(idCuenta);
                var detalleExistente = productos.FirstOrDefault(p => p.IdDetalle == id);
                
                if (detalleExistente == null)
                {
                    return new JsonResult(new { success = false, message = "Detalle no encontrado" });
                }

                // Eliminar el detalle antiguo
                await _detalleConsumoService.EliminarAsync(id);

                // Crear nuevo detalle con la cantidad actualizada
                var nuevoDetalle = new DetalleConsumoCreateDto
                {
                    IdCuenta = idCuenta,
                    IdProducto = detalleExistente.IdProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = detalleExistente.PrecioUnitario
                };

                await _detalleConsumoService.CrearAsync(nuevoDetalle);
                
                var cuenta = await _cuentaService.ObtenerAsync(idCuenta);

                return new JsonResult(new
                {
                    success = true,
                    cuenta = new
                    {
                        subtotalServicios = cuenta?.SubtotalServicios ?? 0,
                        subtotalProductos = cuenta?.SubtotalProductos ?? 0,
                        subtotalConsumos = cuenta?.SubtotalConsumos ?? 0,
                        descuento = cuenta?.Descuento ?? 0,
                        total = cuenta?.Total ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar cantidad del producto");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // DEVOLVER PRODUCTO
        // ============================================
        public async Task<IActionResult> OnPostDevolverProductoAsync(int id, int idCuenta)
        {
            try
            {
                // Obtener información del producto antes de eliminarlo
                var productos = await _detalleConsumoService.ListarPorCuentaAsync(idCuenta);
                var detalle = productos.FirstOrDefault(p => p.IdDetalle == id);
                
                if (detalle == null)
                {
                    return new JsonResult(new { success = false, message = "Detalle no encontrado" });
                }

                // Eliminar el detalle (esto devuelve el stock automáticamente si está configurado en el servicio)
                await _detalleConsumoService.EliminarAsync(id);
                
                var cuenta = await _cuentaService.ObtenerAsync(idCuenta);

                return new JsonResult(new
                {
                    success = true,
                    cuenta = new
                    {
                        subtotalServicios = cuenta?.SubtotalServicios ?? 0,
                        subtotalProductos = cuenta?.SubtotalProductos ?? 0,
                        subtotalConsumos = cuenta?.SubtotalConsumos ?? 0,
                        descuento = cuenta?.Descuento ?? 0,
                        total = cuenta?.Total ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al devolver producto");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // DEVOLVER PARCIAL DE PRODUCTO
        // ============================================
        public async Task<IActionResult> OnPostDevolverParcialAsync(int id, int cantidad, int idCuenta)
        {
            try
            {
                if (cantidad <= 0)
                {
                    return new JsonResult(new { success = false, message = "Cantidad inválida" });
                }

                var productos = await _detalleConsumoService.ListarPorCuentaAsync(idCuenta);
                var detalle = productos.FirstOrDefault(p => p.IdDetalle == id);
                if (detalle == null)
                {
                    return new JsonResult(new { success = false, message = "Detalle no encontrado" });
                }

                var nuevaCantidad = detalle.Cantidad - cantidad;
                if (nuevaCantidad < 0)
                {
                    return new JsonResult(new { success = false, message = "La cantidad a devolver excede la consumida" });
                }

                if (nuevaCantidad == 0)
                {
                    await _detalleConsumoService.EliminarAsync(id);
                }
                else
                {
                    await _detalleConsumoService.ActualizarAsync(id, new DetalleConsumoUpdateDto
                    {
                        Cantidad = nuevaCantidad,
                        PrecioUnitario = detalle.PrecioUnitario
                    });
                }

                var cuenta = await _cuentaService.ObtenerAsync(idCuenta);
                return new JsonResult(new
                {
                    success = true,
                    cuenta = new
                    {
                        subtotalServicios = cuenta?.SubtotalServicios ?? 0,
                        subtotalProductos = cuenta?.SubtotalProductos ?? 0,
                        subtotalConsumos = cuenta?.SubtotalConsumos ?? 0,
                        descuento = cuenta?.Descuento ?? 0,
                        total = cuenta?.Total ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en devolución parcial");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        // ============================================
        // OBTENER DETALLES DE CUENTA
        // ============================================
        public async Task<IActionResult> OnGetObtenerDetallesAsync(int id)
        {
            try
            {
                var servicios = await _detalleServicioService.ListarPorCuentaAsync(id);
                var productos = await _detalleConsumoService.ListarPorCuentaAsync(id);

                return new JsonResult(new
                {
                    success = true,
                    servicios = servicios.Select(s => new
                    {
                        idDetalleServicio = s.IdDetalleServicio,
                        cantidad = s.Cantidad,
                        precioUnitario = s.PrecioUnitario,
                        subtotal = s.Subtotal,
                        servicioNombre = s.ServicioNombre
                    }),
                    productos = productos.Select(p => new
                    {
                        idDetalle = p.IdDetalle,
                        cantidad = p.Cantidad,
                        precioUnitario = p.PrecioUnitario,
                        subtotal = p.Subtotal,
                        productoNombre = p.ProductoNombre
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalles");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnGetBuscarProductosAsync(string term)
        {
            try
            {
                var productos = await _productoService.ListarAsync(term);
                return new JsonResult(new
                {
                    success = true,
                    productos = productos.Select(p => new
                    {
                        idProducto = p.IdProducto,
                        nombre = p.Nombre,
                        precio = p.Precio,
                        stockActual = p.StockActual
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
