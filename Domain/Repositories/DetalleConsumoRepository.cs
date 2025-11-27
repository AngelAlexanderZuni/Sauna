using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class DetalleConsumoRepository : Repository<DetalleConsumo>, IDetalleConsumoRepository
    {
        public DetalleConsumoRepository(SaunaDbContext context) : base(context) { }

        public async Task<IEnumerable<DetalleConsumo>> GetByProductoIdAsync(int productoId)
        {
            return await _context.DetallesConsumo
                .Include(d => d.Producto)
                .Include(d => d.Cuenta)
                .Where(d => d.IdProducto == productoId)
                .OrderByDescending(d => d.IdDetalle)
                .ToListAsync();
        }

        public async Task<IEnumerable<DetalleConsumo>> GetByCuentaIdAsync(int cuentaId)
        {
            return await _context.DetallesConsumo
                .Include(d => d.Producto)
                .Include(d => d.Cuenta)
                .Where(d => d.IdCuenta == cuentaId)
                .OrderByDescending(d => d.IdDetalle)
                .ToListAsync();
        }
    }
}

