using ProyectoSaunaKalixto.Web.Domain.DTOs;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    /// <summary>
    /// Servicio para cálculos de caja y flujo de caja (sin tabla - queries dinámicas)
    /// </summary>
    public interface ICajaService
    {
        /// <summary>
        /// Calcula el cierre de caja para una fecha específica
        /// </summary>
        Task<CierreCajaDTO> CalcularCierreDiarioAsync(DateTime fecha);

        /// <summary>
        /// Obtiene el desglose de ingresos por método de pago para una fecha
        /// </summary>
        Task<List<IngresoPorMetodoDTO>> ObtenerIngresosPorMetodoAsync(DateTime fecha);

        /// <summary>
        /// Obtiene el detalle de todos los pagos del día
        /// </summary>
        Task<List<DetallePagoDTO>> ObtenerDetallePagosDelDiaAsync(DateTime fecha);

        /// <summary>
        /// Busca clientes por nombre o DNI
        /// </summary>
        Task<List<ClienteBusquedaDTO>> BuscarClientesAsync(string termino);

        /// <summary>
        /// Obtiene el detalle completo de un cliente con su historial
        /// </summary>
        Task<ClienteDetalleDTO> ObtenerDetalleClienteAsync(int idCliente);

        /// <summary>
        /// Calcula el flujo de caja mensual
        /// </summary>
        Task<FlujoCajaDTO> CalcularFlujoCajaMensualAsync(int anio, int mes);

        /// <summary>
        /// Obtiene historial de cierres para un rango de fechas
        /// </summary>
        Task<List<CierreCajaDTO>> ObtenerHistorialCierresAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Verifica si hay cuentas pendientes de pago
        /// </summary>
        Task<int> ContarCuentasPendientesAsync();

        /// <summary>
        /// Verifica si hay una caja abierta para hoy
        /// </summary>
        Task<AperturaCajaDTO?> ObtenerAperturaActivaAsync();

        /// <summary>
        /// Abre la caja con saldo inicial
        /// </summary>
        Task<AperturaCajaDTO> AbrirCajaAsync(AperturaCajaDTO apertura);

        /// <summary>
        /// Cierra la caja formalmente con arqueo
        /// </summary>
        Task<CierreFormalCajaDTO> CerrarCajaAsync(CierreFormalCajaDTO cierre);

        /// <summary>
        /// Genera el PDF del cierre de caja
        /// </summary>
        Task<byte[]> GenerarPDFCierreAsync(DateTime fecha);
    }
}
