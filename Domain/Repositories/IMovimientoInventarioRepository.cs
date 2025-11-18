using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IMovimientoInventarioRepository : IRepository<MovimientoInventario>
    {
        Task<MovimientoInventario?> GetUltimoPorProductoAsync(int idProducto);
        Task<IEnumerable<MovimientoInventario>> GetUltimosAsync(int count);
    }
}