using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Mappers;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IPagoService
    {
        Task<PagoDto> RegistrarAsync(PagoCreateDto dto);
        Task<IEnumerable<PagoDto>> ListarPorCuentaAsync(int idCuenta);
        Task<IEnumerable<PagoDto>> ListarRecientesAsync(int cantidad);
    }

    public class PagoService : IPagoService
    {
        private readonly IPagoRepository _repo;
        private readonly IMetodoPagoRepository _metodos;
        private readonly ICuentaRepository _cuentas;

        public PagoService(IPagoRepository repo, IMetodoPagoRepository metodos, ICuentaRepository cuentas)
        {
            _repo = repo; _metodos = metodos; _cuentas = cuentas;
        }

        public async Task<PagoDto> RegistrarAsync(PagoCreateDto dto)
        {
            var cuenta = await _cuentas.GetByIdAsync(dto.IdCuenta) ?? throw new InvalidOperationException("Cuenta no encontrada");
            var metodo = await _metodos.GetByIdAsync(dto.IdMetodoPago) ?? throw new InvalidOperationException("Método de pago no encontrado");
            var pago = new Pago
            {
                IdCuenta = dto.IdCuenta,
                IdMetodoPago = dto.IdMetodoPago,
                Monto = dto.Monto,
                NumeroReferencia = dto.NumeroReferencia,
                FechaHora = DateTime.Now
            };
            var saved = await _repo.AddAsync(pago);
            return PagoMapper.ToDto(saved);
        }

        public async Task<IEnumerable<PagoDto>> ListarPorCuentaAsync(int idCuenta)
        {
            var list = await _repo.GetByCuentaIdAsync(idCuenta);
            return list.Select(PagoMapper.ToDto);
        }

        public async Task<IEnumerable<PagoDto>> ListarRecientesAsync(int cantidad)
        {
            var list = await _repo.GetRecientesAsync(cantidad);
            return list.Select(PagoMapper.ToDto);
        }
    }
}
