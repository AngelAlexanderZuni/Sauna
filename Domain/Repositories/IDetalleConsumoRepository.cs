using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IDetalleConsumoRepository : IRepository<DetalleConsumo>
    {
        Task<IEnumerable<DetalleConsumo>> GetByProductoIdAsync(int productoId);
        Task<IEnumerable<DetalleConsumo>> GetByCuentaIdAsync(int cuentaId);
    }
}

