using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface ICategoriaProductoRepository : IRepository<CategoriaProducto>
    {
        Task<IEnumerable<CategoriaProducto>> GetAllOrderedAsync();
    }
}