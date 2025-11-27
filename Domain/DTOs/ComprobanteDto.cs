using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class ComprobanteDto
    {
        public int IdComprobante { get; set; }
        public string Serie { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public int IdTipoComprobante { get; set; }
        public string TipoNombre { get; set; } = string.Empty;
        public int IdCuenta { get; set; }
    }

    public class ComprobanteCreateDto
    {
        public int IdCuenta { get; set; }
        public int IdTipoComprobante { get; set; }
        public string Serie { get; set; } = string.Empty;
        public string? Numero { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        public DateTime? FechaEmision { get; set; }
    }
}
