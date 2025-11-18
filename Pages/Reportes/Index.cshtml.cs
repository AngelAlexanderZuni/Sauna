using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using System.Text;

namespace ProyectoSaunaKalixto.Web.Pages.Reportes
{
    [Authorize(Roles = "Administrador,Admin")]
    public class IndexModel : PageModel
    {
        private readonly SaunaDbContext _context;

        public IndexModel(SaunaDbContext context)
        {
            _context = context;
        }

        public int TotalProductos { get; set; }
        public int UnidadesTotales { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosSinStock { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public decimal ValorTotalCosto { get; set; }
        public List<TopProducto> TopProductos { get; set; } = new();

        public async Task OnGet()
        {
            var productos = await _context.Productos.Where(p => p.Activo).ToListAsync();
            TotalProductos = productos.Count;
            UnidadesTotales = productos.Sum(p => p.StockActual);
            ProductosStockBajo = productos.Count(p => p.StockActual <= p.StockMinimo && p.StockActual > 0);
            ProductosSinStock = productos.Count(p => p.StockActual == 0);
            ValorTotalInventario = productos.Sum(p => p.StockActual * p.PrecioVenta);
            ValorTotalCosto = productos.Sum(p => p.StockActual * p.PrecioCompra);

            TopProductos = await _context.MovimientoInventario
                .Include(m => m.Producto)
                .Where(m => m.Producto != null)
                .GroupBy(m => new { m.IdProducto, Nombre = m.Producto!.Nombre })
                .Select(g => new TopProducto
                {
                    IdProducto = g.Key.IdProducto,
                    Nombre = g.Key.Nombre,
                    CantidadMovida = g.Sum(x => x.Cantidad),
                    IngresosEstimados = g.Sum(x => x.Cantidad * (x.Producto!.PrecioVenta - x.Producto!.PrecioCompra))
                })
                .OrderByDescending(t => t.CantidadMovida)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetExportInventarioAsync()
        {
            var productos = await _context.Productos.Include(p => p.CategoriaProducto).OrderBy(p => p.Nombre).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Codigo,Nombre,Categoria,PrecioCompra,PrecioVenta,StockActual,StockMinimo");
            foreach (var p in productos)
            {
                sb.AppendLine($"{p.Codigo},{p.Nombre},{p.CategoriaProducto?.Nombre ?? ""},{p.PrecioCompra},{p.PrecioVenta},{p.StockActual},{p.StockMinimo}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "inventario.csv");
        }

        public class TopProducto
        {
            public int IdProducto { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public int CantidadMovida { get; set; }
            public decimal IngresosEstimados { get; set; }
        }
    }
}
