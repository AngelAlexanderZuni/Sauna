using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class MetodoPagoRepository : Repository<MetodoPago>, IMetodoPagoRepository
    {
        public MetodoPagoRepository(SaunaDbContext context) : base(context) { }

        public async Task<MetodoPago?> GetByNombreAsync(string nombre)
        {
            return await _context.MetodosPago.FirstOrDefaultAsync(m => m.Nombre == nombre);
        }
    }
}
