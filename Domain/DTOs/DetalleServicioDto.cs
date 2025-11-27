namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class DetalleServicioDto
    {
        public int IdDetalleServicio { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public int IdCuenta { get; set; }
        public int IdServicio { get; set; }
        public string? ServicioNombre { get; set; }
    }

    public class DetalleServicioCreateDto
    {
        public int IdCuenta { get; set; }
        public int IdServicio { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class DetalleServicioUpdateDto
    {
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}

