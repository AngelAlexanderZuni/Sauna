using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class TipoEgresoRepository : ITipoEgresoRepository
    {
        private readonly SaunaDbContext _context;

        public TipoEgresoRepository(SaunaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TipoEgreso>> GetAllAsync()
        {
            return await _context.TiposEgreso
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        public async Task<TipoEgreso?> GetByIdAsync(int id)
        {
            return await _context.TiposEgreso.FindAsync(id);
        }

        public async Task<TipoEgreso> CreateAsync(TipoEgreso tipoEgreso)
        {
            _context.TiposEgreso.Add(tipoEgreso);
            await _context.SaveChangesAsync();
            return tipoEgreso;
        }

        public async Task<bool> ExistsByNombreAsync(string nombre, int? excludeId = null)
        {
            var query = _context.TiposEgreso.AsQueryable();
            
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.IdTipoEgreso != excludeId.Value);
            }
            
            return await query.AnyAsync(t => t.Nombre.ToLower() == nombre.ToLower());
        }
    }
}
