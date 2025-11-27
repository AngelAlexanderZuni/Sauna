namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class CuentaDto
    {
        public int IdCuenta { get; set; }
        public DateTime FechaHoraCreacion { get; set; }
        public DateTime? FechaHoraSalida { get; set; }
        public decimal SubtotalServicios { get; set; }
        public decimal SubtotalProductos { get; set; }
        public decimal SubtotalConsumos { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public int IdEstadoCuenta { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public int IdUsuarioCreador { get; set; }
        public int IdCliente { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public int? IdPromocion { get; set; }
        public string? PromocionNombre { get; set; }
        public decimal? PromocionMontoDescuento { get; set; }
    }

    public class CuentaCreateDto
    {
        public int IdCliente { get; set; }
        public int IdUsuarioCreador { get; set; }
        public int IdEstadoCuenta { get; set; }
        public int? IdPromocion { get; set; }
    }

    public class CuentaUpdateDto
    {
        public DateTime? FechaHoraSalida { get; set; }
        public int? IdPromocion { get; set; }
    }
}

