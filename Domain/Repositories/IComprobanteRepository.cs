using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IComprobanteRepository : IRepository<Comprobante>
    {
        Task<IEnumerable<Comprobante>> GetByCuentaIdAsync(int cuentaId);
        Task<Comprobante?> GetUltimoPorSerieAsync(string serie);
        Task<IEnumerable<Comprobante>> GetRecientesAsync(int cantidad);
    }
}
