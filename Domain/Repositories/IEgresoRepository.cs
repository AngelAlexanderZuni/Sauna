using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    /// <summary>
    /// Repository para Egresos (CabEgreso + DetEgreso + TipoEgreso)
    /// </summary>
    public interface IEgresoRepository
    {
        // ===== OPERACIONES CRUD COMPLETAS CON TRANSACCIONES =====
        
        /// <summary>
        /// Crear egreso completo (CabEgreso + DetallesEgreso) en UNA transacción
        /// </summary>
        Task<int> CrearEgresoCompletoAsync(CabEgreso cabecera, List<DetEgreso> detalles);
        
        /// <summary>
        /// Obtener todos los egresos con sus detalles incluidos
        /// </summary>
        Task<List<CabEgreso>> ObtenerTodosConDetallesAsync();
        
        /// <summary>
        /// Obtener un egreso por ID con sus detalles
        /// </summary>
        Task<CabEgreso?> ObtenerPorIdConDetallesAsync(int idCabEgreso);
        
        /// <summary>
        /// Obtener detalles de un egreso específico
        /// </summary>
        Task<List<DetEgreso>> ObtenerDetallesPorCabeceraAsync(int idCabEgreso);
        
        /// <summary>
        /// Eliminar egreso completo (cabecera + detalles) en UNA transacción
        /// </summary>
        Task<bool> EliminarEgresoCompletoAsync(int idCabEgreso);
        
        
        // ===== CONSULTAS Y REPORTES =====
        
        /// <summary>
        /// Obtener egresos por rango de fechas con detalles
        /// </summary>
        Task<List<CabEgreso>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
        
        /// <summary>
        /// Calcular total de egresos en un rango de fechas
        /// </summary>
        Task<decimal> ObtenerTotalPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
        
        
        // ===== CATÁLOGOS =====
        
        /// <summary>
        /// Obtener todos los tipos de egreso disponibles (catálogo)
        /// </summary>
        Task<List<TipoEgreso>> ObtenerTiposEgresoAsync();
        Task<TipoEgreso> CrearTipoEgresoAsync(TipoEgreso tipo);
        Task ActualizarTipoEgresoAsync(int id, string nuevoNombre);
        Task<bool> EliminarTipoEgresoAsync(int id);
        Task<bool> TipoTieneEgresosAsync(int idTipo);
    }
}
