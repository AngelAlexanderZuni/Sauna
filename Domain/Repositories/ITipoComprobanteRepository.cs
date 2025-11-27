using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface ITipoComprobanteRepository : IRepository<TipoComprobante>
    {
        Task<TipoComprobante?> GetByNombreAsync(string nombre);
    }
}
