using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IProductoService
    {
        Task<IEnumerable<ProductoDto>> ListarAsync(string? busqueda);
        Task<ProductoDto?> ObtenerAsync(int id);
    }

    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repo;

        public ProductoService(IProductoRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ProductoDto>> ListarAsync(string? busqueda)
        {
            var productos = await _repo.SearchAsync(busqueda, null, null, false);
            return productos.Select(ProductoDto.FromEntity);
        }

        public async Task<ProductoDto?> ObtenerAsync(int id)
        {
            var producto = await _repo.GetByIdAsync(id);
            return producto == null ? null : ProductoDto.FromEntity(producto);
        }
    }
}

