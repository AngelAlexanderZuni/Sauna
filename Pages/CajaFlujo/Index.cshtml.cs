using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.CajaFlujo
{
    [Authorize(Roles = "Administrador,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ICajaService _cajaService;

        public IndexModel(ICajaService cajaService)
        {
            _cajaService = cajaService;
        }

        // Propiedades para la vista
        public CierreCajaDTO? CierreActual { get; set; }
        public List<IngresoPorMetodoDTO> IngresosPorMetodo { get; set; } = new();
        public FlujoCajaDTO? FlujoMensual { get; set; }
        public int CuentasPendientes { get; set; }
        public DateTime FechaSeleccionada { get; set; } = DateTime.Today;
        public bool CajaAbierta { get; set; }
        public decimal SaldoInicial { get; set; }

        // Para vista de flujo mensual
        public string VistaActual { get; set; } = "caja"; // caja o flujo

        public async Task OnGetAsync(string? vista, DateTime? fecha, int? anio, int? mes)
        {
            VistaActual = vista ?? "caja";
            FechaSeleccionada = fecha ?? DateTime.Today;

            // Verificar estado de caja
            var apertura = await _cajaService.ObtenerAperturaActivaAsync();
            CajaAbierta = apertura != null;
            SaldoInicial = apertura?.SaldoInicial ?? 0;

            if (VistaActual == "flujo")
            {
                var anioActual = anio ?? DateTime.Today.Year;
                var mesActual = mes ?? DateTime.Today.Month;
                FlujoMensual = await _cajaService.CalcularFlujoCajaMensualAsync(anioActual, mesActual);
            }
            else
            {
                // Vista de caja diaria
                CierreActual = await _cajaService.CalcularCierreDiarioAsync(FechaSeleccionada);
                IngresosPorMetodo = await _cajaService.ObtenerIngresosPorMetodoAsync(FechaSeleccionada);
                CuentasPendientes = await _cajaService.ContarCuentasPendientesAsync();
            }
        }

        public async Task<IActionResult> OnGetCalcularCierreAsync(DateTime fecha)
        {
            try
            {
                var cierre = await _cajaService.CalcularCierreDiarioAsync(fecha);
                var ingresos = await _cajaService.ObtenerIngresosPorMetodoAsync(fecha);

                return new JsonResult(new
                {
                    success = true,
                    cierre = new
                    {
                        fecha = cierre.Fecha.ToString("dd/MM/yyyy"),
                        totalIngresos = cierre.TotalIngresos,
                        totalEgresos = cierre.TotalEgresos,
                        gananciaNeta = cierre.GananciaNeta,
                        cantidadPagos = cierre.CantidadPagos,
                        cantidadEgresos = cierre.CantidadEgresos,
                        ingresosPorMetodo = ingresos.Select(i => new
                        {
                            metodoPago = i.MetodoPago,
                            montoTotal = i.MontoTotal,
                            cantidadTransacciones = i.CantidadTransacciones
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error al calcular cierre: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> OnGetDetallePagosAsync(DateTime fecha)
        {
            try
            {
                var pagos = await _cajaService.ObtenerDetallePagosDelDiaAsync(fecha);

                return new JsonResult(new
                {
                    success = true,
                    pagos = pagos.Select(p => new
                    {
                        hora = p.FechaPago.ToString("HH:mm"),
                        cliente = p.NombreCliente,
                        metodoPago = p.MetodoPago,
                        monto = p.Monto,
                        comprobante = p.TipoComprobante
                    })
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error al obtener detalle: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> OnGetBuscarClientesAsync(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
                {
                    return new JsonResult(new { success = true, clientes = new List<object>() });
                }

                var clientes = await _cajaService.BuscarClientesAsync(termino);

                return new JsonResult(new
                {
                    success = true,
                    clientes = clientes.Select(c => new
                    {
                        idCliente = c.IdCliente,
                        nombre = c.Nombre,
                        dni = c.Dni,
                        telefono = c.Telefono,
                        saldoPendiente = c.SaldoPendiente
                    })
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error al buscar clientes: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> OnGetDetalleClienteAsync(int idCliente)
        {
            try
            {
                var detalle = await _cajaService.ObtenerDetalleClienteAsync(idCliente);

                return new JsonResult(new
                {
                    success = true,
                    cliente = new
                    {
                        idCliente = detalle.IdCliente,
                        nombre = detalle.Nombre,
                        dni = detalle.Dni,
                        telefono = detalle.Telefono,
                        email = detalle.Email,
                        totalPagado = detalle.TotalPagado,
                        totalConsumos = detalle.TotalConsumos,
                        saldoPendiente = detalle.SaldoPendiente,
                        cantidadVisitas = detalle.CantidadVisitas,
                        ultimaVisita = detalle.UltimaVisita?.ToString("dd/MM/yyyy HH:mm"),
                        historialPagos = detalle.HistorialPagos.Select(p => new
                        {
                            fecha = p.Fecha.ToString("dd/MM/yyyy HH:mm"),
                            monto = p.Monto,
                            metodoPago = p.MetodoPago,
                            numeroReferencia = p.NumeroReferencia
                        }),
                        ultimosConsumos = detalle.UltimosConsumos.Select(c => new
                        {
                            fecha = c.Fecha.ToString("dd/MM/yyyy"),
                            servicio = c.Servicio,
                            monto = c.Monto
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error al obtener detalle del cliente: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> OnGetHistorialAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                var historial = await _cajaService.ObtenerHistorialCierresAsync(fechaInicio, fechaFin);

                return new JsonResult(new
                {
                    success = true,
                    cierres = historial.Select(c => new
                    {
                        fecha = c.Fecha.ToString("dd/MM/yyyy"),
                        totalIngresos = c.TotalIngresos,
                        totalEgresos = c.TotalEgresos,
                        gananciaNeta = c.GananciaNeta,
                        cantidadPagos = c.CantidadPagos,
                        cantidadEgresos = c.CantidadEgresos
                    })
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error al obtener historial: {ex.Message}"
                });
            }
        }

        public async Task<IActionResult> OnPostAbrirCajaAsync([FromBody] AperturaCajaRequest request)
        {
            try
            {
                var idUsuario = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");
                
                var apertura = new AperturaCajaDTO
                {
                    SaldoInicial = request.SaldoInicial,
                    FondoCambio = 0,
                    Observaciones = request.Observaciones,
                    IdUsuario = idUsuario
                };

                await _cajaService.AbrirCajaAsync(apertura);

                return new JsonResult(new { success = true, message = "Caja abierta exitosamente" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostCerrarCajaAsync([FromBody] CierreCajaRequest request)
        {
            try
            {
                var idUsuario = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");
                
                var cierre = new CierreFormalCajaDTO
                {
                    IdUsuario = idUsuario,
                    Arqueo = new ArqueoCajaDTO
                    {
                        EfectivoReal = request.EfectivoReal,
                        Billetes = new Dictionary<string, int>(),
                        Monedas = new Dictionary<string, int>()
                    },
                    DepositoBancario = 0,
                    EfectivoEnCaja = request.EfectivoReal
                };

                await _cajaService.CerrarCajaAsync(cierre);

                return new JsonResult(new { success = true, message = "Caja cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }

    public class AperturaCajaRequest
    {
        public decimal SaldoInicial { get; set; }
        public string? Observaciones { get; set; }
    }

    public class CierreCajaRequest
    {
        public decimal EfectivoReal { get; set; }
    }
}
