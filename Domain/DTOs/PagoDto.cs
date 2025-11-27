using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class PagoDto
    {
        public int IdPago { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal Monto { get; set; }
        public string? NumeroReferencia { get; set; }
        public int IdMetodoPago { get; set; }
        public string MetodoNombre { get; set; } = string.Empty;
        public int IdCuenta { get; set; }
    }

    public class PagoCreateDto
    {
        public int IdCuenta { get; set; }
        public int IdMetodoPago { get; set; }
        public decimal Monto { get; set; }
        public string? NumeroReferencia { get; set; }
    }
}
