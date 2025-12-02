using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;
using System.Globalization;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    /// <summary>
    /// Implementación del servicio de caja - todo calculado con queries SQL dinámicas
    /// </summary>
    public class CajaService : ICajaService
    {
        private readonly SaunaDbContext _context;

        public CajaService(SaunaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calcula el cierre de caja para una fecha específica
        /// </summary>
        public async Task<CierreCajaDTO> CalcularCierreDiarioAsync(DateTime fecha)
        {
            var fechaSoloFecha = fecha.Date;
            var fechaSiguiente = fechaSoloFecha.AddDays(1);

            // Calcular total de ingresos del día
            var totalIngresos = await _context.Pagos
                .Where(p => p.FechaHora >= fechaSoloFecha && p.FechaHora < fechaSiguiente)
                .SumAsync(p => (decimal?)p.Monto) ?? 0m;

            var cantidadPagos = await _context.Pagos
                .Where(p => p.FechaHora >= fechaSoloFecha && p.FechaHora < fechaSiguiente)
                .CountAsync();

            // Calcular total de egresos del día
            var totalEgresos = await _context.CabEgresos
                .Where(e => e.Fecha >= fechaSoloFecha && e.Fecha < fechaSiguiente)
                .SumAsync(e => (decimal?)e.MontoTotal) ?? 0m;

            var cantidadEgresos = await _context.CabEgresos
                .Where(e => e.Fecha >= fechaSoloFecha && e.Fecha < fechaSiguiente)
                .CountAsync();

            // Desglose por método de pago
            var ingresosPorMetodo = await _context.Pagos
                .Where(p => p.FechaHora >= fechaSoloFecha && p.FechaHora < fechaSiguiente)
                .GroupBy(p => p.MetodoPago.Nombre)
                .Select(g => new { Metodo = g.Key, Total = g.Sum(p => p.Monto) })
                .ToDictionaryAsync(x => x.Metodo, x => x.Total);

            return new CierreCajaDTO
            {
                Fecha = fechaSoloFecha,
                TotalIngresos = totalIngresos,
                TotalEgresos = totalEgresos,
                GananciaNeta = totalIngresos - totalEgresos,
                IngresosPorMetodo = ingresosPorMetodo,
                CantidadPagos = cantidadPagos,
                CantidadEgresos = cantidadEgresos
            };
        }

        /// <summary>
        /// Obtiene el desglose de ingresos por método de pago
        /// </summary>
        public async Task<List<IngresoPorMetodoDTO>> ObtenerIngresosPorMetodoAsync(DateTime fecha)
        {
            var fechaSoloFecha = fecha.Date;
            var fechaSiguiente = fechaSoloFecha.AddDays(1);

            var ingresos = await _context.Pagos
                .Where(p => p.FechaHora >= fechaSoloFecha && p.FechaHora < fechaSiguiente)
                .Include(p => p.MetodoPago)
                .GroupBy(p => p.MetodoPago.Nombre)
                .Select(g => new IngresoPorMetodoDTO
                {
                    MetodoPago = g.Key,
                    MontoTotal = g.Sum(p => p.Monto),
                    CantidadTransacciones = g.Count()
                })
                .ToListAsync();

            return ingresos;
        }

        /// <summary>
        /// Obtiene el detalle de todos los pagos del día
        /// </summary>
        public async Task<List<DetallePagoDTO>> ObtenerDetallePagosDelDiaAsync(DateTime fecha)
        {
            var fechaSoloFecha = fecha.Date;
            var fechaSiguiente = fechaSoloFecha.AddDays(1);

            var pagos = await _context.Pagos
                .Where(p => p.FechaHora >= fechaSoloFecha && p.FechaHora < fechaSiguiente)
                .Include(p => p.Cuenta)
                    .ThenInclude(c => c.Cliente)
                .Include(p => p.Cuenta.Comprobantes)
                    .ThenInclude(c => c.TipoComprobante)
                .Include(p => p.MetodoPago)
                .OrderBy(p => p.FechaHora)
                .Select(p => new DetallePagoDTO
                {
                    FechaPago = p.FechaHora,
                    NombreCliente = p.Cuenta.Cliente.Nombre,
                    MetodoPago = p.MetodoPago.Nombre,
                    Monto = p.Monto,
                    TipoComprobante = p.Cuenta.Comprobantes
                        .OrderByDescending(c => c.FechaEmision)
                        .Select(c => c.TipoComprobante.Nombre)
                        .FirstOrDefault() ?? "Sin comprobante"
                })
                .ToListAsync();

            return pagos;
        }

        /// <summary>
        /// Busca clientes por nombre o DNI
        /// </summary>
        public async Task<List<ClienteBusquedaDTO>> BuscarClientesAsync(string termino)
        {
            termino = termino.ToLower().Trim();

            // Buscar clientes
            var clientesIds = await _context.Clientes
                .Where(c => c.Nombre.ToLower().Contains(termino) || 
                           (c.NumeroDocumento != null && c.NumeroDocumento.Contains(termino)))
                .Select(c => c.ClienteID)
                .Take(10)
                .ToListAsync();

            // Calcular saldos para cada cliente
            var clientesConSaldo = new List<ClienteBusquedaDTO>();

            foreach (var clienteId in clientesIds)
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.ClienteID == clienteId);

                if (cliente != null)
                {
                    // Calcular saldo pendiente: Total de cuenta - Total pagado
                    var cuentasCliente = await _context.Cuentas
                        .Where(cu => cu.IdCliente == clienteId && cu.EstadoCuenta.Nombre == "Pendiente")
                        .Include(cu => cu.Pagos)
                        .ToListAsync();

                    var saldoPendiente = cuentasCliente.Sum(cu => cu.Total - cu.Pagos.Sum(p => p.Monto));

                    clientesConSaldo.Add(new ClienteBusquedaDTO
                    {
                        IdCliente = cliente.ClienteID,
                        Nombre = cliente.NombreCompleto,
                        Dni = cliente.NumeroDocumento ?? "S/N",
                        Telefono = cliente.Telefono,
                        SaldoPendiente = saldoPendiente
                    });
                }
            }

            return clientesConSaldo;
        }

        /// <summary>
        /// Obtiene el detalle completo de un cliente con su historial
        /// </summary>
        public async Task<ClienteDetalleDTO> ObtenerDetalleClienteAsync(int idCliente)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.ClienteID == idCliente);

            if (cliente == null)
            {
                throw new Exception("Cliente no encontrado");
            }

            // Obtener las cuentas del cliente
            var cuentas = await _context.Cuentas
                .Where(c => c.IdCliente == idCliente)
                .Include(c => c.Pagos)
                    .ThenInclude(p => p.MetodoPago)
                .Include(c => c.DetallesServicio)
                    .ThenInclude(ds => ds.Servicio)
                .ToListAsync();

            var totalPagado = cuentas.SelectMany(c => c.Pagos).Sum(p => p.Monto);
            var totalConsumos = cuentas.Sum(c => c.Total);
            var saldoPendiente = totalConsumos - totalPagado;

            var historialPagos = cuentas
                .SelectMany(c => c.Pagos)
                .OrderByDescending(p => p.FechaHora)
                .Take(10)
                .Select(p => new PagoHistorialDTO
                {
                    Fecha = p.FechaHora,
                    Monto = p.Monto,
                    MetodoPago = p.MetodoPago.Nombre,
                    NumeroReferencia = p.NumeroReferencia
                })
                .ToList();

            var ultimosConsumos = cuentas
                .SelectMany(c => c.DetallesServicio)
                .OrderByDescending(ds => ds.Cuenta.FechaHoraCreacion)
                .Take(5)
                .Select(ds => new ConsumoHistorialDTO
                {
                    Fecha = ds.Cuenta.FechaHoraCreacion,
                    Servicio = ds.Servicio.Nombre,
                    Monto = ds.PrecioUnitario * ds.Cantidad
                })
                .ToList();

            var cantidadVisitas = cuentas.Count;
            var ultimaVisita = cuentas.Max(c => (DateTime?)c.FechaHoraCreacion);

            return new ClienteDetalleDTO
            {
                IdCliente = cliente.ClienteID,
                Nombre = cliente.NombreCompleto,
                Dni = cliente.NumeroDocumento ?? "S/N",
                Telefono = cliente.Telefono,
                Email = cliente.Correo,
                TotalPagado = totalPagado,
                TotalConsumos = totalConsumos,
                SaldoPendiente = saldoPendiente,
                CantidadVisitas = cantidadVisitas,
                UltimaVisita = ultimaVisita,
                HistorialPagos = historialPagos,
                UltimosConsumos = ultimosConsumos
            };
        }

        /// <summary>
        /// Calcula el flujo de caja mensual
        /// </summary>
        public async Task<FlujoCajaDTO> CalcularFlujoCajaMensualAsync(int anio, int mes)
        {
            var primerDiaMes = new DateTime(anio, mes, 1);
            var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

            // Total ingresos del mes
            var totalIngresos = await _context.Pagos
                .Where(p => p.FechaHora.Year == anio && p.FechaHora.Month == mes)
                .SumAsync(p => (decimal?)p.Monto) ?? 0m;

            // Total egresos del mes
            var totalEgresos = await _context.CabEgresos
                .Where(e => e.Fecha.Year == anio && e.Fecha.Month == mes)
                .SumAsync(e => (decimal?)e.MontoTotal) ?? 0m;

            // Cierres diarios del mes
            var cierresDiarios = new List<CierreCajaDTO>();
            for (var dia = primerDiaMes; dia <= ultimoDiaMes; dia = dia.AddDays(1))
            {
                var cierre = await CalcularCierreDiarioAsync(dia);
                if (cierre.TotalIngresos > 0 || cierre.TotalEgresos > 0)
                {
                    cierresDiarios.Add(cierre);
                }
            }

            var nombreMes = new CultureInfo("es-ES").DateTimeFormat.GetMonthName(mes);

            return new FlujoCajaDTO
            {
                Anio = anio,
                Mes = mes,
                NombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1),
                TotalIngresos = totalIngresos,
                TotalEgresos = totalEgresos,
                UtilidadNeta = totalIngresos - totalEgresos,
                CierresDiarios = cierresDiarios
            };
        }

        /// <summary>
        /// Obtiene historial de cierres para un rango de fechas
        /// </summary>
        public async Task<List<CierreCajaDTO>> ObtenerHistorialCierresAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var cierres = new List<CierreCajaDTO>();

            for (var fecha = fechaInicio.Date; fecha <= fechaFin.Date; fecha = fecha.AddDays(1))
            {
                var cierre = await CalcularCierreDiarioAsync(fecha);
                if (cierre.TotalIngresos > 0 || cierre.TotalEgresos > 0)
                {
                    cierres.Add(cierre);
                }
            }

            return cierres.OrderByDescending(c => c.Fecha).ToList();
        }

        /// <summary>
        /// Verifica si hay cuentas pendientes de pago
        /// </summary>
        public async Task<int> ContarCuentasPendientesAsync()
        {
            return await _context.Cuentas
                .Where(c => c.IdEstadoCuenta == 1) // 1 = Pendiente
                .CountAsync();
        }

        // Variable estática para simular apertura de caja (sin usar base de datos)
        private static AperturaCajaDTO? _aperturaActual = null;

        /// <summary>
        /// Verifica si hay una caja abierta para hoy (SIN BASE DE DATOS - solo en memoria)
        /// </summary>
        public async Task<AperturaCajaDTO?> ObtenerAperturaActivaAsync()
        {
            await Task.CompletedTask; // Para mantener la firma async
            
            // Si hay apertura y es de hoy, retornarla
            if (_aperturaActual != null && _aperturaActual.FechaHoraApertura.Date == DateTime.Today)
            {
                return _aperturaActual;
            }

            return null;
        }

        /// <summary>
        /// Abre la caja con saldo inicial (SIN BASE DE DATOS - solo en memoria)
        /// </summary>
        public async Task<AperturaCajaDTO> AbrirCajaAsync(AperturaCajaDTO dto)
        {
            await Task.CompletedTask; // Para mantener la firma async
            
            // Verificar que no haya caja abierta
            var aperturaActiva = await ObtenerAperturaActivaAsync();
            if (aperturaActiva != null)
            {
                throw new InvalidOperationException("Ya existe una caja abierta para hoy");
            }

            // Guardar en memoria
            _aperturaActual = new AperturaCajaDTO
            {
                FechaHoraApertura = DateTime.Now,
                SaldoInicial = dto.SaldoInicial,
                FondoCambio = dto.FondoCambio,
                Observaciones = dto.Observaciones,
                IdUsuario = dto.IdUsuario,
                NombreUsuario = "Usuario"
            };

            return _aperturaActual;
        }

        /// <summary>
        /// Cierra la caja formalmente con arqueo (SIN BASE DE DATOS - solo limpia memoria)
        /// </summary>
        public async Task<CierreFormalCajaDTO> CerrarCajaAsync(CierreFormalCajaDTO dto)
        {
            // Verificar que haya caja abierta
            if (_aperturaActual == null || _aperturaActual.FechaHoraApertura.Date != DateTime.Today)
            {
                throw new InvalidOperationException("No hay caja abierta para cerrar");
            }

            // Calcular totales del día
            var cierre = await CalcularCierreDiarioAsync(DateTime.Today);

            dto.FechaHoraCierre = DateTime.Now;
            dto.SaldoInicial = _aperturaActual.SaldoInicial;
            dto.TotalIngresos = cierre.TotalIngresos;
            dto.TotalEgresos = cierre.TotalEgresos;
            dto.SaldoEsperado = _aperturaActual.SaldoInicial + cierre.TotalIngresos - cierre.TotalEgresos;
            dto.IngresosPorMetodo = cierre.IngresosPorMetodo;

            // Limpiar apertura de memoria
            _aperturaActual = null;

            return dto;
        }

        /// <summary>
        /// Genera el PDF del cierre de caja (HTML simple sin base de datos)
        /// </summary>
        public async Task<byte[]> GenerarPDFCierreAsync(DateTime fecha)
        {
            var cierre = await CalcularCierreDiarioAsync(fecha);
            var ingresos = await ObtenerIngresosPorMetodoAsync(fecha);

            // Usar apertura de memoria si existe
            var aperturaActiva = await ObtenerAperturaActivaAsync();

            // Generar HTML del PDF
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 20px; }}
        .header {{ text-align: center; border-bottom: 2px solid #333; padding-bottom: 10px; margin-bottom: 20px; }}
        .section {{ margin: 20px 0; }}
        .table {{ width: 100%; border-collapse: collapse; margin: 10px 0; }}
        .table th, .table td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        .table th {{ background-color: #f4f4f4; }}
        .total {{ font-weight: bold; background-color: #f0f0f0; }}
        .right {{ text-align: right; }}
        .footer {{ margin-top: 40px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>CIERRE DE CAJA</h1>
        <h3>Sauna Kalixto</h3>
        <p>Fecha: {fecha:dd/MM/yyyy}</p>
        <p>Hora de generación: {DateTime.Now:HH:mm}</p>
    </div>

    <div class='section'>
        <h3>RESUMEN DEL DÍA</h3>
        <table class='table'>
            <tr>
                <td>Saldo Inicial:</td>
                <td class='right'>S/ {(aperturaActiva?.SaldoInicial ?? 0):N2}</td>
            </tr>
            <tr>
                <td>Total Ingresos:</td>
                <td class='right' style='color: green;'>S/ {cierre.TotalIngresos:N2}</td>
            </tr>
            <tr>
                <td>Total Egresos:</td>
                <td class='right' style='color: red;'>S/ {cierre.TotalEgresos:N2}</td>
            </tr>
            <tr class='total'>
                <td>Saldo Esperado:</td>
                <td class='right'>S/ {((aperturaActiva?.SaldoInicial ?? 0) + cierre.TotalIngresos - cierre.TotalEgresos):N2}</td>
            </tr>
        </table>
    </div>

    <div class='section'>
        <h3>INGRESOS POR MÉTODO DE PAGO</h3>
        <table class='table'>
            <thead>
                <tr>
                    <th>Método de Pago</th>
                    <th class='right'>Transacciones</th>
                    <th class='right'>Monto Total</th>
                </tr>
            </thead>
            <tbody>
                {string.Join("", ingresos.Select(i => $@"
                <tr>
                    <td>{i.MetodoPago}</td>
                    <td class='right'>{i.CantidadTransacciones}</td>
                    <td class='right'>S/ {i.MontoTotal:N2}</td>
                </tr>"))}
                <tr class='total'>
                    <td>TOTAL</td>
                    <td class='right'>{ingresos.Sum(i => i.CantidadTransacciones)}</td>
                    <td class='right'>S/ {cierre.TotalIngresos:N2}</td>
                </tr>
            </tbody>
        </table>
    </div>

    <div class='footer'>
        <p>Este documento fue generado automáticamente el {DateTime.Now:dd/MM/yyyy HH:mm}</p>
        <p>Sistema de Gestión Sauna Kalixto</p>
    </div>
</body>
</html>";

            // Aquí necesitarías una librería de PDF como iTextSharp o DinkToPdf
            // Por ahora retorno el HTML como bytes (puedes usar una librería de PDF después)
            return System.Text.Encoding.UTF8.GetBytes(html);
        }
    }
}
