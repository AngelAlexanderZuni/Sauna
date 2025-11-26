using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IPromocionService
    {
        Task<IEnumerable<PromocionDto>> ListarAsync(string? term);
        Task<PromocionDto?> ObtenerAsync(int id);
        Task<PromocionDto> CrearAsync(PromocionCreateDto dto);
        Task<PromocionDto?> ActualizarAsync(int id, PromocionCreateDto dto);
        Task<bool> EliminarAsync(int id);
        Task<IEnumerable<TipoDescuentoDto>> TiposAsync();
        Task<TipoDescuentoDto> CrearTipoAsync(string nombre);
        Task<TipoDescuentoDto?> ActualizarTipoAsync(int id, string nombre);
        Task<bool> EliminarTipoAsync(int id);
    }

    public class PromocionService : IPromocionService
    {
        private readonly IPromocionRepository _promRepo;
        private readonly ITipoDescuentoRepository _tipoRepo;

        public PromocionService(IPromocionRepository promRepo, ITipoDescuentoRepository tipoRepo)
        {
            _promRepo = promRepo;
            _tipoRepo = tipoRepo;
        }

        public async Task<IEnumerable<PromocionDto>> ListarAsync(string? term)
        {
            var list = await _promRepo.SearchAsync(term);
            return list.Select(Map);
        }

        public async Task<PromocionDto?> ObtenerAsync(int id)
        {
            var p = await _promRepo.GetByIdAsync(id);
            return p == null ? null : Map(p);
        }

        public async Task<PromocionDto> CrearAsync(PromocionCreateDto dto)
        {
            Validar(dto);
            var entity = new Promocion
            {
                NombreDescuento = dto.NombreDescuento.Trim(),
                MontoDescuento = dto.MontoDescuento,
                IdTipoDescuento = dto.IdTipoDescuento,
                ValorCondicion = dto.ValorCondicion,
                Activo = dto.Activo,
                Motivo = dto.Motivo?.Trim()
            };
            var saved = await _promRepo.AddAsync(entity);
            return Map(saved);
        }

        public async Task<PromocionDto?> ActualizarAsync(int id, PromocionCreateDto dto)
        {
            var p = await _promRepo.GetByIdAsync(id);
            if (p == null) return null;
            Validar(dto);
            p.NombreDescuento = dto.NombreDescuento.Trim();
            p.MontoDescuento = dto.MontoDescuento;
            p.IdTipoDescuento = dto.IdTipoDescuento;
            p.ValorCondicion = dto.ValorCondicion;
            p.Activo = dto.Activo;
            p.Motivo = dto.Motivo?.Trim();
            await _promRepo.UpdateAsync(p);
            return Map(p);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            await _promRepo.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<TipoDescuentoDto>> TiposAsync()
        {
            var tipos = await _tipoRepo.GetAllAsync();
            return tipos.Select(t => new TipoDescuentoDto 
            { 
                IdTipoDescuento = t.IdTipoDescuento, 
                Nombre = t.Nombre 
            });
        }

        // MÉTODOS PARA GESTIONAR TIPOS DE DESCUENTO
        public async Task<TipoDescuentoDto> CrearTipoAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido");

            var entity = new TipoDescuento { Nombre = nombre.Trim() };
            var saved = await _tipoRepo.AddAsync(entity);
            return new TipoDescuentoDto 
            { 
                IdTipoDescuento = saved.IdTipoDescuento, 
                Nombre = saved.Nombre 
            };
        }

        public async Task<TipoDescuentoDto?> ActualizarTipoAsync(int id, string nombre)
        {
            var tipo = await _tipoRepo.GetByIdAsync(id);
            if (tipo == null) return null;

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido");

            tipo.Nombre = nombre.Trim();
            await _tipoRepo.UpdateAsync(tipo);
            return new TipoDescuentoDto 
            { 
                IdTipoDescuento = tipo.IdTipoDescuento, 
                Nombre = tipo.Nombre 
            };
        }

        public async Task<bool> EliminarTipoAsync(int id)
        {
            await _tipoRepo.DeleteAsync(id);
            return true;
        }

        private void Validar(PromocionCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreDescuento)) 
                throw new ArgumentException("Nombre requerido");
            if (dto.MontoDescuento < 0) 
                throw new ArgumentException("Monto no puede ser negativo");
        }

        private PromocionDto Map(Promocion p)
        {
            return new PromocionDto
            {
                IdPromocion = p.IdPromocion,
                NombreDescuento = p.NombreDescuento,
                MontoDescuento = p.MontoDescuento,
                ValorCondicion = p.ValorCondicion,
                Activo = p.Activo,
                Motivo = p.Motivo,
                IdTipoDescuento = p.IdTipoDescuento,
                TipoNombre = p.TipoDescuento?.Nombre ?? string.Empty
            };
        }
    }
}