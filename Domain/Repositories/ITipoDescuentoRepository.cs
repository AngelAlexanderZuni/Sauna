using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface ITipoDescuentoRepository : IRepository<TipoDescuento>
    {
        Task<TipoDescuento?> GetByNameAsync(string nombre);
    }
}

