using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class CategoriaProductoRepository : Repository<CategoriaProducto>, ICategoriaProductoRepository
    {
        public CategoriaProductoRepository(SaunaDbContext context) : base(context) { }

        public async Task<IEnumerable<CategoriaProducto>> GetAllOrderedAsync()
        {
            return await _context.CategoriasProducto.OrderBy(c => c.Nombre).ToListAsync();
        }
    }
}