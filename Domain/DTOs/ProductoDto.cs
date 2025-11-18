using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class ProductoDto
    {
        public int IdProducto { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public int? IdCategoriaProducto { get; set; }
        public bool Activo { get; set; }
        public string? CategoriaNombre { get; set; }
        public static ProductoDto FromEntity(Producto p)
        {
            return new ProductoDto
            {
                IdProducto = p.IdProducto,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioCompra = p.PrecioCompra,
                PrecioVenta = p.PrecioVenta,
                StockActual = p.StockActual,
                StockMinimo = p.StockMinimo,
                IdCategoriaProducto = p.IdCategoriaProducto,
                Activo = p.Activo,
                CategoriaNombre = p.CategoriaProducto?.Nombre
            };
        }
    }
}