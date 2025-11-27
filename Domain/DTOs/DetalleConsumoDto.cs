namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class DetalleConsumoDto
    {
        public int IdDetalle { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public int IdCuenta { get; set; }
        public int IdProducto { get; set; }
        public string? ProductoNombre { get; set; }
        public string? ProductoCodigo { get; set; }
    }

    public class DetalleConsumoCreateDto
    {
        public int IdCuenta { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }

    public class DetalleConsumoUpdateDto
    {
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}

