using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<IEnumerable<Cliente>> GetByNumeroDocumentoAsync(string numeroDocumento);
        Task<bool> ExistsByNumeroDocumentoAsync(string numeroDocumento);
        Task<IEnumerable<Cliente>> SearchAsync(string searchTerm);
    }
}