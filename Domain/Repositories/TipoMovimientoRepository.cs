using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class TipoMovimientoRepository : Repository<TipoMovimiento>, ITipoMovimientoRepository
    {
        public TipoMovimientoRepository(SaunaDbContext context) : base(context) { }

        public async Task<TipoMovimiento> GetOrCreateAsync(string nombre)
        {
            var tipo = await _context.TiposMovimiento.FirstOrDefaultAsync(t => t.Nombre == nombre);
            if (tipo != null) return tipo;
            tipo = new TipoMovimiento { Nombre = nombre };
            await _context.TiposMovimiento.AddAsync(tipo);
            return tipo;
        }
    }
}