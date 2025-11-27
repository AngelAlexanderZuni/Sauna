using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    /// <summary>
    /// Repository para manejo de Egresos (CabEgreso + DetEgreso + TipoEgreso)
    /// con transacciones para garantizar consistencia de datos
    /// </summary>
    public class EgresoRepository : IEgresoRepository
    {
        private readonly SaunaDbContext _context;

        public EgresoRepository(SaunaDbContext context)
        {
            _context = context;
        }

        // ===== CREAR EGRESO COMPLETO CON TRANSACCIÓN =====
        
        /// <summary>
        /// Crea un egreso completo (cabecera + detalles) en UNA transacción
        /// Garantiza que ambos se inserten o ninguno
        /// </summary>
        public async Task<int> CrearEgresoCompletoAsync(CabEgreso cabecera, List<DetEgreso> detalles)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Log antes de insertar
                Console.WriteLine($"📋 Insertando CabEgreso: Fecha={cabecera.Fecha}, MontoTotal={cabecera.MontoTotal}, IdUsuario={cabecera.IdUsuario}");
                
                // 1. Insertar CabEgreso
                _context.CabEgresos.Add(cabecera);
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"✅ CabEgreso insertado con ID={cabecera.IdCabEgreso}");

                // 2. Asignar IdCabEgreso a cada detalle
                foreach (var detalle in detalles)
                {
                    detalle.IdCabEgreso = cabecera.IdCabEgreso;
                    Console.WriteLine($"   📝 Detalle: IdCabEgreso={detalle.IdCabEgreso}, Concepto={detalle.Concepto}, Monto={detalle.Monto}, IdTipoEgreso={detalle.IdTipoEgreso}");
                }

                // 3. Insertar todos los DetEgreso
                _context.DetEgresos.AddRange(detalles);
                Console.WriteLine($"💾 Guardando {detalles.Count} detalles...");
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ Detalles guardados correctamente");

                // 4. Commit de la transacción
                await transaction.CommitAsync();

                return cabecera.IdCabEgreso;
            }
            catch (Exception ex)
            {
                // Rollback en caso de error
                await transaction.RollbackAsync();
                throw new Exception($"Error al crear egreso completo: {ex.Message}", ex);
            }
        }

        // ===== CONSULTAS =====
        
        /// <summary>
        /// Obtiene todos los egresos con sus detalles y relaciones
        /// </summary>
        public async Task<List<CabEgreso>> ObtenerTodosConDetallesAsync()
        {
            return await _context.CabEgresos
                .Include(c => c.Usuario)
                .Include(c => c.DetallesEgreso)
                    .ThenInclude(d => d.TipoEgreso)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un egreso por ID con sus detalles
        /// </summary>
        public async Task<CabEgreso?> ObtenerPorIdConDetallesAsync(int idCabEgreso)
        {
            return await _context.CabEgresos
                .Include(c => c.Usuario)
                .Include(c => c.DetallesEgreso)
                    .ThenInclude(d => d.TipoEgreso)
                .FirstOrDefaultAsync(c => c.IdCabEgreso == idCabEgreso);
        }

        /// <summary>
        /// Obtiene solo los detalles de un egreso específico
        /// </summary>
        public async Task<List<DetEgreso>> ObtenerDetallesPorCabeceraAsync(int idCabEgreso)
        {
            return await _context.DetEgresos
                .Include(d => d.TipoEgreso)
                .Where(d => d.IdCabEgreso == idCabEgreso)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene egresos por rango de fechas
        /// </summary>
        public async Task<List<CabEgreso>> ObtenerPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.CabEgresos
                .Include(c => c.Usuario)
                .Include(c => c.DetallesEgreso)
                    .ThenInclude(d => d.TipoEgreso)
                .Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin)
                .OrderByDescending(c => c.Fecha)
                .ToListAsync();
        }

        /// <summary>
        /// Calcula el total de egresos en un rango de fechas
        /// </summary>
        public async Task<decimal> ObtenerTotalPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var total = await _context.CabEgresos
                .Where(c => c.Fecha >= fechaInicio && c.Fecha <= fechaFin)
                .SumAsync(c => (decimal?)c.MontoTotal);

            return total ?? 0;
        }

        // ===== ELIMINAR EGRESO COMPLETO CON TRANSACCIÓN =====
        
        /// <summary>
        /// Elimina un egreso completo (cabecera + detalles) en UNA transacción
        /// </summary>
        public async Task<bool> EliminarEgresoCompletoAsync(int idCabEgreso)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Buscar la cabecera con sus detalles
                var cabEgreso = await _context.CabEgresos
                    .Include(c => c.DetallesEgreso)
                    .FirstOrDefaultAsync(c => c.IdCabEgreso == idCabEgreso);

                if (cabEgreso == null)
                    return false;

                // 2. Eliminar detalles primero
                _context.DetEgresos.RemoveRange(cabEgreso.DetallesEgreso);

                // 3. Eliminar cabecera
                _context.CabEgresos.Remove(cabEgreso);

                // 4. Guardar cambios
                await _context.SaveChangesAsync();

                // 5. Commit
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Error al eliminar egreso: {ex.Message}", ex);
            }
        }

        // ===== CATÁLOGOS =====
        
        /// <summary>
        /// Obtiene todos los tipos de egreso disponibles
        /// </summary>
        public async Task<List<TipoEgreso>> ObtenerTiposEgresoAsync()
        {
            return await _context.TiposEgreso
                .OrderBy(t => t.Nombre)
                .ToListAsync();
        }

        // ===== GESTIÓN DE TIPOS DE EGRESO =====

        public async Task<TipoEgreso> CrearTipoEgresoAsync(TipoEgreso tipo)
        {
            _context.TiposEgreso.Add(tipo);
            await _context.SaveChangesAsync();
            return tipo;
        }

        public async Task ActualizarTipoEgresoAsync(int id, string nuevoNombre)
        {
            var tipo = await _context.TiposEgreso.FindAsync(id);
            if (tipo != null)
            {
                tipo.Nombre = nuevoNombre;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> EliminarTipoEgresoAsync(int id)
        {
            var tipo = await _context.TiposEgreso.FindAsync(id);
            if (tipo == null) return false;

            _context.TiposEgreso.Remove(tipo);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> TipoTieneEgresosAsync(int idTipo)
        {
            return await _context.DetEgresos.AnyAsync(d => d.IdTipoEgreso == idTipo);
        }
    }
}
