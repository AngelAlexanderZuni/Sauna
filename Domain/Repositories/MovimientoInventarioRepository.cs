using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class MovimientoInventarioRepository : Repository<MovimientoInventario>, IMovimientoInventarioRepository
    {
        public MovimientoInventarioRepository(SaunaDbContext context) : base(context) { }

        public async Task<MovimientoInventario?> GetUltimoPorProductoAsync(int idProducto)
        {
            return await _context.MovimientoInventario
                .Where(m => m.IdProducto == idProducto)
                .OrderByDescending(m => m.Fecha)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MovimientoInventario>> GetUltimosAsync(int count)
        {
            return await _context.MovimientoInventario
                .Include(m => m.Producto)
                .Include(m => m.TipoMovimiento)
                .OrderByDescending(m => m.Fecha)
                .Take(count)
                .ToListAsync();
        }
    }
}