using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class CategoriaServicioRepository : Repository<CategoriaServicio>, ICategoriaServicioRepository
    {
        public CategoriaServicioRepository(SaunaDbContext context) : base(context) { }

        public async Task<IEnumerable<CategoriaServicio>> GetAllOrderedAsync()
        {
            return await _context.CategoriasServicio.OrderBy(c => c.Nombre).ToListAsync();
        }
    }
}