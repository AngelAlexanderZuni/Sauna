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
        private readonly ILogger<PromocionService>? _logger;

        public PromocionService(IPromocionRepository promRepo, ITipoDescuentoRepository tipoRepo, ILogger<PromocionService>? logger = null)
        {
            _promRepo = promRepo;
            _tipoRepo = tipoRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<PromocionDto>> ListarAsync(string? term)
        {
            var list = await _promRepo.SearchAsync(term);
            return list.Select(Mappers.PromocionMapper.ToDto);
        }

        public async Task<PromocionDto?> ObtenerAsync(int id)
        {
            var p = await _promRepo.GetByIdAsync(id);
            return p == null ? null : Mappers.PromocionMapper.ToDto(p);
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
            Promocion saved;
            try
            {
                saved = await _promRepo.AddAsync(entity);
                _logger?.LogInformation("Promoción creada {Id} - {Nombre}", saved.IdPromocion, saved.NombreDescuento);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creando promoción {Nombre}", entity.NombreDescuento);
                throw;
            }
            return Mappers.PromocionMapper.ToDto(saved);
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
            try
            {
                await _promRepo.UpdateAsync(p);
                _logger?.LogInformation("Promoción actualizada {Id}", p.IdPromocion);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error actualizando promoción {Id}", id);
                throw;
            }
            return Mappers.PromocionMapper.ToDto(p);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                await _promRepo.DeleteAsync(id);
                _logger?.LogInformation("Promoción eliminada {Id}", id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error eliminando promoción {Id}", id);
                throw;
            }
            return true;
        }

        public async Task<IEnumerable<TipoDescuentoDto>> TiposAsync()
        {
            var tipos = await _tipoRepo.GetAllAsync();
            return tipos.Select(Mappers.TipoDescuentoMapper.ToDto);
        }

        // MÉTODOS PARA GESTIONAR TIPOS DE DESCUENTO
        public async Task<TipoDescuentoDto> CrearTipoAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido");

            var entity = new TipoDescuento { Nombre = nombre.Trim() };
            var saved = await _tipoRepo.AddAsync(entity);
            return Mappers.TipoDescuentoMapper.ToDto(saved);
        }

        public async Task<TipoDescuentoDto?> ActualizarTipoAsync(int id, string nombre)
        {
            var tipo = await _tipoRepo.GetByIdAsync(id);
            if (tipo == null) return null;

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es requerido");

            tipo.Nombre = nombre.Trim();
            await _tipoRepo.UpdateAsync(tipo);
            return Mappers.TipoDescuentoMapper.ToDto(tipo);
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

        
    }
}
