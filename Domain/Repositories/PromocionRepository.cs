using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class PromocionRepository : Repository<Promocion>, IPromocionRepository
    {
        public PromocionRepository(SaunaDbContext context) : base(context) { }

        public async Task<IEnumerable<Promocion>> SearchAsync(string? term)
        {
            var q = _context.Promociones.Include(p => p.TipoDescuento).AsQueryable();
            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.Trim();
                q = q.Where(p => EF.Functions.Like(p.NombreDescuento, "%" + term + "%") ||
                                  (p.Motivo != null && EF.Functions.Like(p.Motivo, "%" + term + "%")));
            }
            return await q.OrderBy(p => p.NombreDescuento).ToListAsync();
        }
    }
}

