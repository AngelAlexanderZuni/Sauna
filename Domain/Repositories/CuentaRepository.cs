using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class CuentaRepository : Repository<Cuenta>, ICuentaRepository
    {
        public CuentaRepository(SaunaDbContext context) : base(context) { }

        // AGREGAR ESTE MÉTODO NUEVO
        public override async Task<Cuenta> AddAsync(Cuenta entity)
        {
            await _context.Cuentas.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            // Recargar con todas las relaciones
            var cuentaGuardada = await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.EstadoCuenta)
                .Include(c => c.Promocion)
                    .ThenInclude(p => p.TipoDescuento)
                .Include(c => c.UsuarioCreador)
                .FirstAsync(c => c.IdCuenta == entity.IdCuenta);
                
            return cuentaGuardada;
        }

        public async Task<IEnumerable<Cuenta>> GetOpenAsync()
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.EstadoCuenta)
                .Include(c => c.Promocion)
                .Where(c => c.FechaHoraSalida == null)
                .OrderByDescending(c => c.FechaHoraCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cuenta>> GetByClienteAsync(int idCliente, bool abiertasSolo)
        {
            var q = _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.EstadoCuenta)
                .Include(c => c.Promocion)
                .Where(c => c.IdCliente == idCliente);
            if (abiertasSolo) q = q.Where(c => c.FechaHoraSalida == null);
            return await q.OrderByDescending(c => c.FechaHoraCreacion).ToListAsync();
        }

        public async Task<Cuenta?> GetWithRelationsAsync(int id)
        {
            return await _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.EstadoCuenta)
                .Include(c => c.Promocion)
                .FirstOrDefaultAsync(c => c.IdCuenta == id);
        }

        public async Task<IEnumerable<Cuenta>> ListWithRelationsAsync(bool abiertasSolo)
        {
            var q = _context.Cuentas
                .Include(c => c.Cliente)
                .Include(c => c.EstadoCuenta)
                .Include(c => c.Promocion)
                .AsQueryable();
            if (abiertasSolo) q = q.Where(c => c.FechaHoraSalida == null);
            return await q.OrderByDescending(c => c.FechaHoraCreacion).ToListAsync();
        }
    }
}