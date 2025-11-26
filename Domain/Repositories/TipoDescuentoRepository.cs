using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class TipoDescuentoRepository : Repository<TipoDescuento>, ITipoDescuentoRepository
    {
        public TipoDescuentoRepository(SaunaDbContext context) : base(context) { }

        public async Task<TipoDescuento?> GetByNameAsync(string nombre)
        {
            return await _context.TiposDescuento.FirstOrDefaultAsync(t => t.Nombre == nombre);
        }
    }
}

