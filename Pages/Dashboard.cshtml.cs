using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.Services;
using ProyectoSaunaKalixto.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace ProyectoSaunaKalixto.Web.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly IClienteService _clienteService;
        private readonly SaunaDbContext _context;

        public DashboardModel(ILogger<DashboardModel> logger, IClienteService clienteService, SaunaDbContext context)
        {
            _logger = logger;
            _clienteService = clienteService;
            _context = context;
        }

        public int TotalClientes { get; set; }
        public int ClientesActivos { get; set; }
        public int CuentasAbiertas { get; set; }
        public int RegistrosHoy { get; set; }
        public string UsuarioNombre { get; set; } = string.Empty;
        public string UsuarioRol { get; set; } = string.Empty;

        public decimal IngresosHoy { get; set; }
        public decimal EgresosHoy { get; set; }
        public List<string> ChartFechas { get; set; } = new();
        public List<decimal> ChartIngresos { get; set; } = new();

        public class PagoReciente
        {
            public DateTime Fecha { get; set; }
            public string Cliente { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public int IdCuenta { get; set; }
        }
        public List<PagoReciente> PagosRecientes { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                // Obtener información del usuario actual
                UsuarioNombre = User.Identity?.Name ?? "Usuario";
                UsuarioRol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Usuario";

                var clientes = await _context.Clientes.ToListAsync();
                TotalClientes = clientes.Count;
                ClientesActivos = clientes.Count(c => c.Activo);
                RegistrosHoy = clientes.Count(c => c.FechaRegistro.Date == DateTime.Today);

                CuentasAbiertas = await _context.Cuentas.CountAsync(c => c.FechaHoraSalida == null);
                IngresosHoy = await _context.Pagos.Where(p => p.FechaHora.Date == DateTime.Today).SumAsync(p => p.Monto);
                EgresosHoy = await _context.CabEgresos
                    .Join(_context.DetEgresos, c => c.IdCabEgreso, d => d.IdCabEgreso, (c, d) => new { c.Fecha, d.Monto })
                    .Where(x => x.Fecha.Date == DateTime.Today)
                    .SumAsync(x => x.Monto);

                // Últimos 10 pagos
                PagosRecientes = await _context.Pagos
                    .OrderByDescending(p => p.FechaHora)
                    .Take(10)
                    .Join(_context.Cuentas, p => p.IdCuenta, c => c.IdCuenta, (p, c) => new { p, c.IdCliente })
                    .Join(_context.Clientes, pc => pc.IdCliente, cl => cl.ClienteID, (pc, cl) => new PagoReciente
                    {
                        Fecha = pc.p.FechaHora,
                        Cliente = (cl.Nombre + " " + cl.Apellido).Trim(),
                        Monto = pc.p.Monto,
                        IdCuenta = pc.p.IdCuenta
                    })
                    .ToListAsync();

                // Ingresos últimos 30 días
                var start = DateTime.Today.AddDays(-29);
                var days = Enumerable.Range(0, 30).Select(i => start.AddDays(i).Date).ToList();
                ChartFechas = days.Select(d => d.ToString("dd/MM")).ToList();
                var ingresosPorDia = await _context.Pagos
                    .Where(p => p.FechaHora.Date >= start && p.FechaHora.Date <= DateTime.Today)
                    .GroupBy(p => p.FechaHora.Date)
                    .Select(g => new { Fecha = g.Key, Total = g.Sum(x => x.Monto) })
                    .ToListAsync();
                ChartIngresos = days.Select(d => ingresosPorDia.FirstOrDefault(x => x.Fecha == d)?.Total ?? 0m).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas del dashboard");
                TotalClientes = 0;
                ClientesActivos = 0;
                CuentasAbiertas = 0;
                RegistrosHoy = 0;
                IngresosHoy = 0;
                EgresosHoy = 0;
                ChartFechas = new();
                ChartIngresos = new();
            }
        }
    }
}