using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IProductoRepository : IRepository<Producto>
    {
        Task<IEnumerable<Producto>> SearchAsync(string? busqueda, int? categoriaId, string? estadoStock, bool includeInactive);
        Task<IEnumerable<Producto>> GetActivosAsync();
    }
}