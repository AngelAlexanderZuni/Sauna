using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface ICuentaRepository : IRepository<Cuenta>
    {
        Task<IEnumerable<Cuenta>> GetOpenAsync();
        Task<IEnumerable<Cuenta>> GetByClienteAsync(int idCliente, bool abiertasSolo);
        Task<Cuenta?> GetWithRelationsAsync(int id);
        Task<IEnumerable<Cuenta>> ListWithRelationsAsync(bool abiertasSolo);
    }
}

