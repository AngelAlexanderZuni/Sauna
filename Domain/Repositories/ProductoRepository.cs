using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class ProductoRepository : Repository<Producto>, IProductoRepository
    {
        public ProductoRepository(SaunaDbContext context) : base(context) { }

        public async Task<IEnumerable<Producto>> GetActivosAsync()
        {
            return await _context.Productos.Where(p => p.Activo).OrderBy(p => p.Nombre).ToListAsync();
        }

        public async Task<IEnumerable<Producto>> SearchAsync(string? busqueda, int? categoriaId, string? estadoStock, bool includeInactive)
        {
            var query = _context.Productos.Include(p => p.CategoriaProducto).AsQueryable();
            if (!includeInactive) query = query.Where(p => p.Activo);
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var term = busqueda.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Nombre, "%" + term + "%") ||
                    EF.Functions.Like(p.Codigo, "%" + term + "%") ||
                    (p.Descripcion != null && EF.Functions.Like(p.Descripcion, "%" + term + "%")));
            }
            if (categoriaId.HasValue && categoriaId > 0)
            {
                query = query.Where(p => p.IdCategoriaProducto == categoriaId);
            }
            if (!string.IsNullOrWhiteSpace(estadoStock))
            {
                switch (estadoStock.ToLower())
                {
                    case "bajo":
                        query = query.Where(p => p.StockActual <= p.StockMinimo && p.StockActual > 0);
                        break;
                    case "sinstock":
                        query = query.Where(p => p.StockActual == 0);
                        break;
                    case "normal":
                        query = query.Where(p => p.StockActual > p.StockMinimo);
                        break;
                }
            }
            return await query.OrderBy(p => p.Nombre).ToListAsync();
        }
    }
}