using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class ServicioRepository : Repository<Servicio>, IServicioRepository
    {
        public ServicioRepository(SaunaDbContext context) : base(context) { }

        public async Task<IEnumerable<Servicio>> GetActivosAsync()
        {
            return await _context.Servicios.Where(s => s.Activo).OrderBy(s => s.Nombre).ToListAsync();
        }

        public async Task<IEnumerable<Servicio>> SearchAsync(string? busqueda, int? categoriaId, bool includeInactive, decimal? precioMin, decimal? precioMax, int? duracionMin, int? duracionMax)
        {
            var query = _context.Servicios.Include(s => s.CategoriaServicio).AsQueryable();
            // Si includeInactive es true, mostrar SOLO inactivos; si es false, SOLO activos
            query = includeInactive ? query.Where(s => !s.Activo) : query.Where(s => s.Activo);
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var term = busqueda.Trim();
                query = query.Where(s => EF.Functions.Like(s.Nombre, "%" + term + "%"));
            }
            if (categoriaId.HasValue && categoriaId > 0)
            {
                query = query.Where(s => s.IdCategoriaServicio == categoriaId);
            }
            if (precioMin.HasValue)
            {
                query = query.Where(s => s.Precio >= precioMin.Value);
            }
            if (precioMax.HasValue)
            {
                query = query.Where(s => s.Precio <= precioMax.Value);
            }
            if (duracionMin.HasValue)
            {
                query = query.Where(s => s.DuracionEstimada >= duracionMin.Value);
            }
            if (duracionMax.HasValue)
            {
                query = query.Where(s => s.DuracionEstimada <= duracionMax.Value);
            }
            return await query.OrderBy(s => s.Nombre).ToListAsync();
        }

        public async Task<Servicio?> GetByIdWithCategoriaAsync(int id)
        {
            return await _context.Servicios
                .Include(s => s.CategoriaServicio)
                .FirstOrDefaultAsync(s => s.IdServicio == id);
        }

        public async Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null)
        {
            var query = _context.Servicios.AsQueryable();
            if (excludeId.HasValue) query = query.Where(s => s.IdServicio != excludeId.Value);
            return await query.AnyAsync(s => s.Nombre.ToLower() == nombre.ToLower());
        }
    }
}