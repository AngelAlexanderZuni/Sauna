using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Mappers;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface ICuentaService
    {
        Task<CuentaDto> CrearAsync(CuentaCreateDto dto);
        Task<CuentaDto?> ObtenerAsync(int id);
        Task<IEnumerable<CuentaDto>> ListarAsync(bool abiertasSolo);
        Task<IEnumerable<CuentaDto>> ListarPorClienteAsync(int idCliente, bool abiertasSolo);
        Task<CuentaDto?> CerrarAsync(int id, DateTime fechaSalida);
        Task RecalcularTotalesAsync(int idCuenta);
        Task<bool> EliminarAsync(int id);
    }

    public class CuentaService : ICuentaService
    {
        private readonly ICuentaRepository _repo;
        private readonly IDetalleServicioRepository _detRepo;
        private readonly IDetalleConsumoRepository _consRepo;
        private readonly IUsuarioRepository _usuarios;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _http;
        private readonly ProyectoSaunaKalixto.Web.Data.SaunaDbContext _db;

        public CuentaService(ICuentaRepository repo, IDetalleServicioRepository detRepo, IDetalleConsumoRepository consRepo, IUsuarioRepository usuarios, Microsoft.AspNetCore.Http.IHttpContextAccessor http, ProyectoSaunaKalixto.Web.Data.SaunaDbContext db)
        {
            _repo = repo;
            _detRepo = detRepo;
            _consRepo = consRepo;
            _usuarios = usuarios;
            _http = http;
            _db = db;
        }

        public async Task<CuentaDto> CrearAsync(CuentaCreateDto dto)
        {
            // Asegurar valores obligatorios
            var idUsuario = dto.IdUsuarioCreador;
            if (idUsuario == 0)
            {
                var nombreUsuario = _http?.HttpContext?.User?.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(nombreUsuario))
                {
                    var u = await _usuarios.GetByIdAsync(nombreUsuario);
                    if (u != null) idUsuario = u.IdUsuario;
                }
            }
            var idEstado = dto.IdEstadoCuenta;
            if (idEstado == 0)
            {
                var estado = await _db.EstadosCuenta.OrderBy(e => e.IdEstadoCuenta).FirstOrDefaultAsync();
                if (estado != null) idEstado = estado.IdEstadoCuenta;
            }

            if (dto.IdCliente <= 0) throw new ArgumentException("IdCliente requerido");

            var c = new Cuenta
            {
                FechaHoraCreacion = DateTime.Now,
                SubtotalConsumos = 0,
                Descuento = 0,
                Total = 0,
                IdEstadoCuenta = idEstado,
                IdUsuarioCreador = idUsuario,
                IdCliente = dto.IdCliente,
                IdPromocion = dto.IdPromocion
            };
            var saved = await _repo.AddAsync(c);
            return CuentaMapper.ToDto(saved);
        }

        public async Task<CuentaDto?> ObtenerAsync(int id)
        {
            var c = await _repo.GetWithRelationsAsync(id);
            if (c == null) return null;
            var dto = CuentaMapper.ToDto(c);
            var servs = await _detRepo.FindAsync(d => d.IdCuenta == id);
            var prods = await _consRepo.FindAsync(d => d.IdCuenta == id);
            dto.SubtotalServicios = servs.Sum(d => d.Subtotal);
            dto.SubtotalProductos = prods.Sum(d => d.Subtotal);
            dto.SubtotalConsumos = dto.SubtotalServicios + dto.SubtotalProductos;
            dto.Total = dto.SubtotalConsumos - dto.Descuento;
            return dto;
        }

        public async Task<IEnumerable<CuentaDto>> ListarAsync(bool abiertasSolo)
        {
            var list = await _repo.ListWithRelationsAsync(abiertasSolo);
            var result = new List<CuentaDto>();
            foreach (var c in list)
            {
                var dto = CuentaMapper.ToDto(c);
                var servs = await _detRepo.FindAsync(d => d.IdCuenta == c.IdCuenta);
                var prods = await _consRepo.FindAsync(d => d.IdCuenta == c.IdCuenta);
                dto.SubtotalServicios = servs.Sum(d => d.Subtotal);
                dto.SubtotalProductos = prods.Sum(d => d.Subtotal);
                dto.SubtotalConsumos = dto.SubtotalServicios + dto.SubtotalProductos;
                dto.Total = dto.SubtotalConsumos - dto.Descuento;
                result.Add(dto);
            }
            return result;
        }

        public async Task<IEnumerable<CuentaDto>> ListarPorClienteAsync(int idCliente, bool abiertasSolo)
        {
            var list = await _repo.GetByClienteAsync(idCliente, abiertasSolo);
            return list.Select(CuentaMapper.ToDto);
        }

        public async Task<CuentaDto?> CerrarAsync(int id, DateTime fechaSalida)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null) return null;
            c.FechaHoraSalida = fechaSalida;
            await _repo.UpdateAsync(c);
            return CuentaMapper.ToDto(c);
        }

        public async Task RecalcularTotalesAsync(int idCuenta)
        {
            var c = await _repo.GetByIdAsync(idCuenta);
            if (c == null) return;
            var servicios = await _detRepo.FindAsync(d => d.IdCuenta == idCuenta);
            var productos = await _consRepo.FindAsync(d => d.IdCuenta == idCuenta);
            var subtotalServicios = servicios.Sum(d => d.Subtotal);
            var subtotalProductos = productos.Sum(d => d.Subtotal);
            var subtotal = subtotalServicios + subtotalProductos;
            c.SubtotalConsumos = subtotal;
            var descuento = c.Descuento;
            c.Total = subtotal - descuento;
            await _repo.UpdateAsync(c);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var exists = await _repo.GetByIdAsync(id);
            if (exists == null) return false;
            await _repo.DeleteAsync(id);
            return true;
        }
    }
}

