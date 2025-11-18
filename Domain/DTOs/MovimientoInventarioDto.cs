using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class MovimientoInventarioDto
    {
        public int IdMovimiento { get; set; }
        public int IdProducto { get; set; }
        public string? ProductoNombre { get; set; }
        public int IdTipoMovimiento { get; set; }
        public string? TipoNombre { get; set; }
        public int Cantidad { get; set; }
        public DateTime Fecha { get; set; }
        public string? Observacion { get; set; }
        public string? NumeroDetalle { get; set; }
        public static MovimientoInventarioDto FromEntity(MovimientoInventario m)
        {
            return new MovimientoInventarioDto
            {
                IdMovimiento = m.IdMovimiento,
                IdProducto = m.IdProducto,
                ProductoNombre = m.Producto?.Nombre,
                IdTipoMovimiento = m.IdTipoMovimiento,
                TipoNombre = m.TipoMovimiento?.Nombre,
                Cantidad = m.Cantidad,
                Fecha = m.Fecha,
                Observacion = m.Observacion
            };
        }
    }
}