using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IDetalleServicioRepository : IRepository<DetalleServicio>
    {
        Task<IEnumerable<DetalleServicio>> GetByServicioIdAsync(int servicioId);
        Task<IEnumerable<DetalleServicio>> GetByCuentaIdAsync(int cuentaId);
    }
}
