using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    /// <summary>
    /// Service para lógica de negocio de Egresos (CabEgreso + DetEgreso + TipoEgreso)
    /// </summary>
    public class EgresoService
    {
        private readonly IEgresoRepository _egresoRepository;
        private readonly ILogger<EgresoService> _logger;

        public EgresoService(IEgresoRepository egresoRepository, ILogger<EgresoService> logger)
        {
            _egresoRepository = egresoRepository;
            _logger = logger;
        }

        // ===== CREAR =====
        
        public async Task<EgresoDTO> CrearEgresoAsync(EgresoCreateDTO createDto, int idUsuario)
        {
            _logger.LogInformation("📝 Creando egreso para usuario {UserId}", idUsuario);
            _logger.LogInformation("📅 Fecha: {Fecha}", createDto.Fecha);
            _logger.LogInformation("📊 Detalles: {Count}", createDto.Detalles?.Count ?? 0);
            
            if (createDto.Detalles == null || !createDto.Detalles.Any())
                throw new ArgumentException("Debe agregar al menos un detalle de egreso");

            var montoTotal = createDto.Detalles.Sum(d => d.Monto);
            if (montoTotal <= 0)
                throw new ArgumentException("El monto total debe ser mayor a 0");

            var cabecera = new CabEgreso
            {
                Fecha = createDto.Fecha,
                MontoTotal = montoTotal,
                IdUsuario = idUsuario
            };

            var detalles = createDto.Detalles.Select(d => new DetEgreso
            {
                Concepto = d.Concepto.Trim(),
                Monto = d.Monto,
                Recurrente = d.Recurrente,
                ComprobanteRuta = d.ComprobanteRuta?.Trim(),
                IdTipoEgreso = d.IdTipoEgreso
            }).ToList();

            var idCabEgreso = await _egresoRepository.CrearEgresoCompletoAsync(cabecera, detalles);
            var egresoCreado = await _egresoRepository.ObtenerPorIdConDetallesAsync(idCabEgreso);

            _logger.LogInformation("✅ Egreso creado: ID {Id}, Total S/ {Total:N2}", idCabEgreso, montoTotal);
            
            return MapToDTO(egresoCreado!);
        }

        // ===== CONSULTAS =====
        
        public async Task<List<EgresoDTO>> ObtenerTodosAsync()
        {
            var egresos = await _egresoRepository.ObtenerTodosConDetallesAsync();
            return egresos.Select(MapToDTO).ToList();
        }

        public async Task<EgresoDTO?> ObtenerPorIdAsync(int id)
        {
            var egreso = await _egresoRepository.ObtenerPorIdConDetallesAsync(id);
            return egreso != null ? MapToDTO(egreso) : null;
        }

        public async Task<List<EgresoDTO>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var egresos = await _egresoRepository.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            return egresos.Select(MapToDTO).ToList();
        }

        // ===== ACTUALIZAR =====
        
        public async Task<EgresoDTO> ActualizarEgresoAsync(int idCabEgreso, EgresoCreateDTO updateDto, int idUsuario)
        {
            _logger.LogInformation("✏️ Actualizando egreso {Id}", idCabEgreso);
            
            if (updateDto.Detalles == null || !updateDto.Detalles.Any())
                throw new ArgumentException("Debe agregar al menos un detalle de egreso");

            var montoTotal = updateDto.Detalles.Sum(d => d.Monto);
            if (montoTotal <= 0)
                throw new ArgumentException("El monto total debe ser mayor a 0");

            // Eliminar el egreso antiguo (con sus detalles en cascada)
            await _egresoRepository.EliminarEgresoCompletoAsync(idCabEgreso);

            // Crear el egreso actualizado con el mismo ID conceptualmente (nuevo en BD)
            var cabecera = new CabEgreso
            {
                Fecha = updateDto.Fecha,
                MontoTotal = montoTotal,
                IdUsuario = idUsuario
            };

            var detalles = updateDto.Detalles.Select(d => new DetEgreso
            {
                Concepto = d.Concepto.Trim(),
                Monto = d.Monto,
                Recurrente = d.Recurrente,
                ComprobanteRuta = d.ComprobanteRuta?.Trim(),
                IdTipoEgreso = d.IdTipoEgreso
            }).ToList();

            var nuevoId = await _egresoRepository.CrearEgresoCompletoAsync(cabecera, detalles);
            var egresoActualizado = await _egresoRepository.ObtenerPorIdConDetallesAsync(nuevoId);

            _logger.LogInformation("✅ Egreso actualizado: ID {Id}, Total S/ {Total:N2}", nuevoId, montoTotal);
            
            return MapToDTO(egresoActualizado!);
        }

        // ===== ELIMINAR =====
        
        public async Task<bool> EliminarAsync(int id)
        {
            _logger.LogInformation("🗑️ Eliminando egreso {Id}", id);
            var resultado = await _egresoRepository.EliminarEgresoCompletoAsync(id);
            
            if (resultado)
                _logger.LogInformation("✅ Egreso {Id} eliminado", id);
            else
                _logger.LogWarning("❌ No se pudo eliminar egreso {Id}", id);
            
            return resultado;
        }

        // ===== CATÁLOGOS =====
        
        public async Task<List<TipoEgreso>> ObtenerTiposEgresoAsync()
        {
            return await _egresoRepository.ObtenerTiposEgresoAsync();
        }

        // ===== REPORTES =====
        
        public async Task<decimal> ObtenerTotalPorRangoAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _egresoRepository.ObtenerTotalPorRangoFechasAsync(fechaInicio, fechaFin);
        }

        // ===== GESTIÓN DE TIPOS DE EGRESO =====

        public async Task<TipoEgreso> CrearTipoEgresoAsync(TipoEgreso tipo)
        {
            _logger.LogInformation("📝 Creando tipo de egreso: {Nombre}", tipo.Nombre);
            var nuevoTipo = await _egresoRepository.CrearTipoEgresoAsync(tipo);
            _logger.LogInformation("✅ Tipo de egreso creado: ID {Id}", nuevoTipo.IdTipoEgreso);
            return nuevoTipo;
        }

        public async Task ActualizarTipoEgresoAsync(int id, string nuevoNombre)
        {
            _logger.LogInformation("✏️ Actualizando tipo de egreso ID {Id}", id);
            await _egresoRepository.ActualizarTipoEgresoAsync(id, nuevoNombre);
            _logger.LogInformation("✅ Tipo de egreso actualizado");
        }

        public async Task<bool> EliminarTipoEgresoAsync(int id)
        {
            _logger.LogInformation("🗑️ Eliminando tipo de egreso ID {Id}", id);
            var resultado = await _egresoRepository.EliminarTipoEgresoAsync(id);
            
            if (resultado)
                _logger.LogInformation("✅ Tipo de egreso eliminado");
            else
                _logger.LogWarning("❌ No se pudo eliminar tipo de egreso {Id}", id);
                
            return resultado;
        }

        public async Task<bool> TipoTieneEgresosAsync(int idTipo)
        {
            return await _egresoRepository.TipoTieneEgresosAsync(idTipo);
        }

        // ===== MAPEO =====
        
        private EgresoDTO MapToDTO(CabEgreso cabEgreso)
        {
            return new EgresoDTO
            {
                IdCabEgreso = cabEgreso.IdCabEgreso,
                Fecha = cabEgreso.Fecha,
                MontoTotal = cabEgreso.MontoTotal,
                IdUsuario = cabEgreso.IdUsuario,
                NombreUsuario = cabEgreso.Usuario?.NombreUsuario,
                Detalles = cabEgreso.DetallesEgreso.Select(d => new DetalleEgresoDTO
                {
                    IdDetEgreso = d.IdDetEgreso,
                    Concepto = d.Concepto,
                    Monto = d.Monto,
                    Recurrente = d.Recurrente,
                    ComprobanteRuta = d.ComprobanteRuta,
                    IdTipoEgreso = d.IdTipoEgreso,
                    NombreTipoEgreso = d.TipoEgreso?.Nombre
                }).ToList()
            };
        }
    }
}
