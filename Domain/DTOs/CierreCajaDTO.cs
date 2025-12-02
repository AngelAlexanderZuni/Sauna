namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    /// <summary>
    /// DTO para representar el cierre de caja calculado dinámicamente
    /// </summary>
    public class CierreCajaDTO
    {
        public DateTime Fecha { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal GananciaNeta { get; set; }
        public Dictionary<string, decimal> IngresosPorMetodo { get; set; } = new();
        public int CantidadPagos { get; set; }
        public int CantidadEgresos { get; set; }
    }

    /// <summary>
    /// DTO para desglose de ingresos por método de pago
    /// </summary>
    public class IngresoPorMetodoDTO
    {
        public string MetodoPago { get; set; } = string.Empty;
        public decimal MontoTotal { get; set; }
        public int CantidadTransacciones { get; set; }
    }

    /// <summary>
    /// DTO para detalle de un pago individual
    /// </summary>
    public class DetallePagoDTO
    {
        public DateTime FechaPago { get; set; }
        public string NombreCliente { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string TipoComprobante { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para búsqueda de clientes en caja
    /// </summary>
    public class ClienteBusquedaDTO
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public decimal SaldoPendiente { get; set; }
    }

    /// <summary>
    /// DTO para detalle completo del cliente
    /// </summary>
    public class ClienteDetalleDTO
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal TotalConsumos { get; set; }
        public decimal SaldoPendiente { get; set; }
        public int CantidadVisitas { get; set; }
        public DateTime? UltimaVisita { get; set; }
        public List<PagoHistorialDTO> HistorialPagos { get; set; } = new();
        public List<ConsumoHistorialDTO> UltimosConsumos { get; set; } = new();
    }

    /// <summary>
    /// DTO para historial de pagos del cliente
    /// </summary>
    public class PagoHistorialDTO
    {
        public DateTime Fecha { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = string.Empty;
        public string? NumeroReferencia { get; set; }
    }

    /// <summary>
    /// DTO para últimos consumos del cliente
    /// </summary>
    public class ConsumoHistorialDTO
    {
        public DateTime Fecha { get; set; }
        public string Servicio { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }

    /// <summary>
    /// DTO para flujo de caja mensual
    /// </summary>
    public class FlujoCajaDTO
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal UtilidadNeta { get; set; }
        public List<CierreCajaDTO> CierresDiarios { get; set; } = new();
    }

    /// <summary>
    /// DTO para apertura de caja
    /// </summary>
    public class AperturaCajaDTO
    {
        public DateTime FechaHoraApertura { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal FondoCambio { get; set; }
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// DTO para arqueo de caja
    /// </summary>
    public class ArqueoCajaDTO
    {
        public decimal EfectivoEsperado { get; set; }
        public decimal EfectivoReal { get; set; }
        public decimal Diferencia { get; set; }
        public Dictionary<string, int> Billetes { get; set; } = new();
        public Dictionary<string, int> Monedas { get; set; } = new();
        public string? MotivoDiferencia { get; set; }
    }

    /// <summary>
    /// DTO para cierre formal de caja
    /// </summary>
    public class CierreFormalCajaDTO
    {
        public DateTime FechaHoraCierre { get; set; }
        public decimal SaldoInicial { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal SaldoEsperado { get; set; }
        public ArqueoCajaDTO Arqueo { get; set; } = new();
        public Dictionary<string, decimal> IngresosPorMetodo { get; set; } = new();
        public int IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public decimal DepositoBancario { get; set; }
        public decimal EfectivoEnCaja { get; set; }
    }
}
