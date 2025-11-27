using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IMetodoPagoRepository : IRepository<MetodoPago>
    {
        Task<MetodoPago?> GetByNombreAsync(string nombre);
    }
}
