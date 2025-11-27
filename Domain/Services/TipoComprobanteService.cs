using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Mappers;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface ITipoComprobanteService
    {
        Task<IEnumerable<TipoComprobanteDto>> ListarAsync();
    }

    public class TipoComprobanteService : ITipoComprobanteService
    {
        private readonly ITipoComprobanteRepository _repo;
        public TipoComprobanteService(ITipoComprobanteRepository repo){ _repo = repo; }
        public async Task<IEnumerable<TipoComprobanteDto>> ListarAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(TipoComprobanteMapper.ToDto);
        }
    }
}
