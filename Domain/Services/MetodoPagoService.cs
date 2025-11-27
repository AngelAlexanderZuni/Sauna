using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Mappers;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IMetodoPagoService
    {
        Task<IEnumerable<MetodoPagoDto>> ListarAsync();
    }

    public class MetodoPagoService : IMetodoPagoService
    {
        private readonly IMetodoPagoRepository _repo;
        public MetodoPagoService(IMetodoPagoRepository repo){ _repo = repo; }
        public async Task<IEnumerable<MetodoPagoDto>> ListarAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MetodoPagoMapper.ToDto);
        }
    }
}
