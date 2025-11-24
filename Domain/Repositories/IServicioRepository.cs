using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public interface IServicioRepository : IRepository<Servicio>
    {
        Task<IEnumerable<Servicio>> GetActivosAsync();
        Task<IEnumerable<Servicio>> SearchAsync(string? busqueda, int? categoriaId, bool includeInactive, decimal? precioMin, decimal? precioMax, int? duracionMin, int? duracionMax);
        Task<Servicio?> GetByIdWithCategoriaAsync(int id);
        Task<bool> ExistsByNameAsync(string nombre, int? excludeId = null);
    }
}