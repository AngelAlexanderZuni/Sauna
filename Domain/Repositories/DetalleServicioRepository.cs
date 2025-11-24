using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class DetalleServicioRepository : Repository<DetalleServicio>, IDetalleServicioRepository
    {
        public DetalleServicioRepository(SaunaDbContext context) : base(context) { }

        public async Task<IEnumerable<DetalleServicio>> GetByServicioIdAsync(int servicioId)
        {
            return await _context.DetallesServicio
                .Include(d => d.Cuenta)
                .Where(d => d.IdServicio == servicioId)
                .OrderByDescending(d => d.Cuenta.FechaHoraCreacion)
                .ToListAsync();
        }
    }
}