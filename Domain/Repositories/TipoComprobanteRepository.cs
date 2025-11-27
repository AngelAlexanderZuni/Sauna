using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class TipoComprobanteRepository : Repository<TipoComprobante>, ITipoComprobanteRepository
    {
        public TipoComprobanteRepository(SaunaDbContext context) : base(context) { }

        public async Task<TipoComprobante?> GetByNombreAsync(string nombre)
        {
            return await _context.TiposComprobante.FirstOrDefaultAsync(t => t.Nombre == nombre);
        }
    }
}
