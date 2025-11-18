using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface ITipoMovimientoRepository : IRepository<TipoMovimiento>
    {
        Task<TipoMovimiento> GetOrCreateAsync(string nombre);
    }
}