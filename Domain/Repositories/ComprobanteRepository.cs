using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class ComprobanteRepository : Repository<Comprobante>, IComprobanteRepository
    {
        public ComprobanteRepository(SaunaDbContext context) : base(context) { }

        public async Task<IEnumerable<Comprobante>> GetByCuentaIdAsync(int cuentaId)
        {
            return await _context.Comprobantes
                .Include(c => c.TipoComprobante)
                .Include(c => c.Cuenta)
                .Where(c => c.IdCuenta == cuentaId)
                .OrderByDescending(c => c.FechaEmision)
                .ToListAsync();
        }

        public async Task<Comprobante?> GetUltimoPorSerieAsync(string serie)
        {
            return await _context.Comprobantes
                .Where(c => c.Serie == serie)
                .OrderByDescending(c => c.IdComprobante)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Comprobante>> GetRecientesAsync(int cantidad)
        {
            return await _context.Comprobantes
                .Include(c => c.TipoComprobante)
                .Include(c => c.Cuenta)
                .OrderByDescending(c => c.FechaEmision)
                .Take(cantidad)
                .ToListAsync();
        }
    }
}
