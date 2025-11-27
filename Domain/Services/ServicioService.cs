using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IServicioService
    {
        Task<IEnumerable<ServicioDto>> ListarAsync(string? busqueda);
        Task<ServicioDto?> ObtenerAsync(int id);
    }

    public class ServicioService : IServicioService
    {
        private readonly IServicioRepository _repo;

        public ServicioService(IServicioRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ServicioDto>> ListarAsync(string? busqueda)
        {
            var servicios = await _repo.SearchAsync(busqueda, null, false, null, null, null, null);
            return servicios.Select(ServicioDto.FromEntity);
        }

        public async Task<ServicioDto?> ObtenerAsync(int id)
        {
            var servicio = await _repo.GetByIdAsync(id);
            return servicio == null ? null : ServicioDto.FromEntity(servicio);
        }
    }
}

