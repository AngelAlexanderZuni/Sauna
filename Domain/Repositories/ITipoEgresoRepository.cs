using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface ITipoEgresoRepository
    {
        Task<IEnumerable<TipoEgreso>> GetAllAsync();
        Task<TipoEgreso?> GetByIdAsync(int id);
        Task<TipoEgreso> CreateAsync(TipoEgreso tipoEgreso);
        Task<bool> ExistsByNombreAsync(string nombre, int? excludeId = null);
    }
}
