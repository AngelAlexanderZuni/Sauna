using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly IClienteService _clienteService;

        public DashboardModel(ILogger<DashboardModel> logger, IClienteService clienteService)
        {
            _logger = logger;
            _clienteService = clienteService;
        }

        public int TotalClientes { get; set; }
        public int ClientesActivos { get; set; }
        public int MembresiasPorVencer { get; set; }
        public int RegistrosHoy { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string UsuarioRol { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            try
            {
                // Obtener información del usuario actual
                UsuarioNombre = User.Identity?.Name ?? "Usuario";
                UsuarioRol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Usuario";

                // Temporal: Usar valores predeterminados hasta que la tabla Clientes exista
                TotalClientes = 0;
                ClientesActivos = 0;
                MembresiasPorVencer = 0;
                RegistrosHoy = 0;
                
                // Descomentar cuando la tabla Clientes esté creada:
                // var clientes = await _clienteService.GetAllClientesAsync();
                // TotalClientes = clientes.Count();
                // ClientesActivos = clientes.Count(c => c.Activo);
                // 
                // // Calcular membresías por vencer (vencen en los próximos 7 días)
                // var fechaLimite = DateTime.Now.AddDays(7);
                // MembresiasPorVencer = clientes.Count(c => c.Activo && 
                //     c.FechaFinMembresia.HasValue && 
                //     c.FechaFinMembresia.Value <= fechaLimite && 
                //     c.FechaFinMembresia.Value >= DateTime.Now);
                // 
                // // Registros de hoy (simulado con clientes registrados hoy)
                // RegistrosHoy = clientes.Count(c => c.FechaRegistro.Date == DateTime.Today);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas del dashboard");
                TotalClientes = 0;
                ClientesActivos = 0;
                MembresiasPorVencer = 0;
                RegistrosHoy = 0;
            }
        }
    }
}