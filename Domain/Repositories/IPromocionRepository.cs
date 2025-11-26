using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IPromocionRepository : IRepository<Promocion>
    {
        Task<IEnumerable<Promocion>> SearchAsync(string? term);
    }
}

