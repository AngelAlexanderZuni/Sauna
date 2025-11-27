using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Mappers;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IComprobanteService
    {
        Task<ComprobanteDto> EmitirAsync(ComprobanteCreateDto dto);
        Task<string> GenerarNumeroAsync(string serie);
        Task<IEnumerable<ComprobanteDto>> ListarPorCuentaAsync(int idCuenta);
        Task<IEnumerable<ComprobanteDto>> ListarRecientesAsync(int cantidad);
    }

    public class ComprobanteService : IComprobanteService
    {
        private readonly IComprobanteRepository _repo;
        private readonly ITipoComprobanteRepository _tipos;
        private readonly ICuentaRepository _cuentas;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public ComprobanteService(IComprobanteRepository repo, ITipoComprobanteRepository tipos, ICuentaRepository cuentas, Microsoft.Extensions.Configuration.IConfiguration config)
        { _repo = repo; _tipos = tipos; _cuentas = cuentas; _config = config; }

        public async Task<ComprobanteDto> EmitirAsync(ComprobanteCreateDto dto)
        {
            var cuenta = await _cuentas.GetByIdAsync(dto.IdCuenta) ?? throw new InvalidOperationException("Cuenta no encontrada");
            var tipo = await _tipos.GetByIdAsync(dto.IdTipoComprobante) ?? throw new InvalidOperationException("Tipo de comprobante no encontrado");
            var serie = string.IsNullOrWhiteSpace(dto.Serie)
                ? (_config.GetValue<string>($"Comprobantes:SeriesPorTipo:{tipo.Nombre}", "F001") ?? "F001")
                : dto.Serie ?? "F001";
            var numero = string.IsNullOrWhiteSpace(dto.Numero) ? await GenerarNumeroAsync(serie) : dto.Numero!;
            var comp = new Comprobante
            {
                IdCuenta = dto.IdCuenta,
                IdTipoComprobante = dto.IdTipoComprobante,
                Serie = serie,
                Numero = numero,
                FechaEmision = dto.FechaEmision ?? DateTime.Now,
                Subtotal = dto.Subtotal,
                Igv = dto.Igv,
                Total = dto.Total
            };

            if (comp.Total > 0 && comp.Subtotal == 0 && comp.Igv == 0)
            {
                var rate = _config.GetValue<decimal>("Comprobantes:IGV", 0.18m);
                var subtotalCalc = Math.Round(comp.Total / (1 + rate), 2);
                var igvCalc = Math.Round(comp.Total - subtotalCalc, 2);
                comp.Subtotal = subtotalCalc;
                comp.Igv = igvCalc;
            }
            var saved = await _repo.AddAsync(comp);
            return ComprobanteMapper.ToDto(saved);
        }

        public async Task<string> GenerarNumeroAsync(string serie)
        {
            var ultimo = await _repo.GetUltimoPorSerieAsync(serie);
            var lastNumber = 0;
            if (ultimo != null && int.TryParse(ultimo.Numero, out var n)) lastNumber = n;
            var next = lastNumber + 1;
            return next.ToString().PadLeft(8, '0');
        }

        public async Task<IEnumerable<ComprobanteDto>> ListarPorCuentaAsync(int idCuenta)
        {
            var list = await _repo.GetByCuentaIdAsync(idCuenta);
            return list.Select(ComprobanteMapper.ToDto);
        }

        public async Task<IEnumerable<ComprobanteDto>> ListarRecientesAsync(int cantidad)
        {
            var list = await _repo.GetRecientesAsync(cantidad);
            return list.Select(ComprobanteMapper.ToDto);
        }
    }
}
