using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Mappers;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IDetalleServicioService
    {
        Task<DetalleServicioDto> CrearAsync(DetalleServicioCreateDto dto);
        Task<DetalleServicioDto?> ActualizarAsync(int id, DetalleServicioUpdateDto dto);
        Task<bool> EliminarAsync(int id);
        Task<IEnumerable<DetalleServicioDto>> ListarPorCuentaAsync(int idCuenta);
    }

    public class DetalleServicioService : IDetalleServicioService
    {
        private readonly IDetalleServicioRepository _repo;
        private readonly ICuentaService _cuentas;

        public DetalleServicioService(IDetalleServicioRepository repo, ICuentaService cuentas)
        {
            _repo = repo; _cuentas = cuentas;
        }

        public async Task<DetalleServicioDto> CrearAsync(DetalleServicioCreateDto dto)
        {
            Validar(dto.Cantidad, dto.PrecioUnitario);
            var entity = new DetalleServicio
            {
                IdCuenta = dto.IdCuenta,
                IdServicio = dto.IdServicio,
                Cantidad = dto.Cantidad,
                PrecioUnitario = dto.PrecioUnitario,
                Subtotal = dto.Cantidad * dto.PrecioUnitario
            };
            var saved = await _repo.AddAsync(entity);
            await _cuentas.RecalcularTotalesAsync(dto.IdCuenta);
            return DetalleServicioMapper.ToDto(saved);
        }

        public async Task<DetalleServicioDto?> ActualizarAsync(int id, DetalleServicioUpdateDto dto)
        {
            Validar(dto.Cantidad, dto.PrecioUnitario);
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return null;
            d.Cantidad = dto.Cantidad;
            d.PrecioUnitario = dto.PrecioUnitario;
            d.Subtotal = dto.Cantidad * dto.PrecioUnitario;
            await _repo.UpdateAsync(d);
            await _cuentas.RecalcularTotalesAsync(d.IdCuenta);
            return DetalleServicioMapper.ToDto(d);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return false;
            var idCuenta = d.IdCuenta;
            await _repo.DeleteAsync(id);
            await _cuentas.RecalcularTotalesAsync(idCuenta);
            return true;
        }

        public async Task<IEnumerable<DetalleServicioDto>> ListarPorCuentaAsync(int idCuenta)
        {
            var list = await _repo.GetByCuentaIdAsync(idCuenta);
            return list.Select(DetalleServicioMapper.ToDto);
        }

        private static void Validar(int cantidad, decimal precio)
        {
            if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser > 0");
            if (precio < 0) throw new ArgumentException("Precio no puede ser negativo");
        }
    }
}

