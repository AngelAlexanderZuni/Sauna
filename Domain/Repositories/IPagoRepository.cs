using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IPagoRepository : IRepository<Pago>
    {
        Task<IEnumerable<Pago>> GetByCuentaIdAsync(int cuentaId);
        Task<Pago?> GetUltimoPorCuentaAsync(int cuentaId);
        Task<IEnumerable<Pago>> GetRecientesAsync(int cantidad);
    }
}
