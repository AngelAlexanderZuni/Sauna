using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class PagoRepository : Repository<Pago>, IPagoRepository
    {
        public PagoRepository(SaunaDbContext context) : base(context) { }

        public async Task<IEnumerable<Pago>> GetByCuentaIdAsync(int cuentaId)
        {
            return await _context.Pagos
                .Include(p => p.MetodoPago)
                .Include(p => p.Cuenta)
                .Where(p => p.IdCuenta == cuentaId)
                .OrderByDescending(p => p.FechaHora)
                .ToListAsync();
        }

        public async Task<Pago?> GetUltimoPorCuentaAsync(int cuentaId)
        {
            return await _context.Pagos
                .Where(p => p.IdCuenta == cuentaId)
                .OrderByDescending(p => p.FechaHora)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Pago>> GetRecientesAsync(int cantidad)
        {
            return await _context.Pagos
                .Include(p => p.MetodoPago)
                .Include(p => p.Cuenta)
                .OrderByDescending(p => p.FechaHora)
                .Take(cantidad)
                .ToListAsync();
        }
    }
}
