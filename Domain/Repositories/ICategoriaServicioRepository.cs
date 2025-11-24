using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface ICategoriaServicioRepository : IRepository<CategoriaServicio>
    {
        Task<IEnumerable<CategoriaServicio>> GetAllOrderedAsync();
    }
}