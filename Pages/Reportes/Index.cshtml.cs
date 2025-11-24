using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using System.Text;
using System.Text.Json;

namespace ProyectoSaunaKalixto.Web.Pages.Reportes
{
    [Authorize(Roles = "Administrador,Admin")]
    public class IndexModel : PageModel
    {
        private readonly SaunaDbContext _context;

        public IndexModel(SaunaDbContext context)
        {
            _context = context;
        }

        public int TotalProductos { get; set; }
        public int UnidadesTotales { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosSinStock { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public decimal ValorTotalCosto { get; set; }
        public List<TopProducto> TopProductos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateTime? Desde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? Hasta { get; set; }

        public decimal Ingresos { get; set; }
        public decimal Egresos { get; set; }
        public decimal Utilidad => Ingresos - Egresos;
        public List<TopServicio> TopServicios { get; set; } = new();

        public List<string> ChartProductoLabels { get; set; } = new();
        public List<int> ChartProductoValues { get; set; } = new();
        public List<string> ChartServicioLabels { get; set; } = new();
        public List<int> ChartServicioValues { get; set; } = new();
        public List<string> ChartFechas { get; set; } = new();
        public List<decimal> ChartIngresosDia { get; set; } = new();
        public List<decimal> ChartEgresosDia { get; set; } = new();

        public async Task OnGet()
        {
            var productos = await _context.Productos.Where(p => p.Activo).ToListAsync();
            TotalProductos = productos.Count;
            UnidadesTotales = productos.Sum(p => p.StockActual);
            ProductosStockBajo = productos.Count(p => p.StockActual <= p.StockMinimo && p.StockActual > 0);
            ProductosSinStock = productos.Count(p => p.StockActual == 0);
            ValorTotalInventario = productos.Sum(p => p.StockActual * p.PrecioVenta);
            ValorTotalCosto = productos.Sum(p => p.StockActual * p.PrecioCompra);

            TopProductos = await _context.MovimientoInventario
                .Include(m => m.Producto)
                .Where(m => m.Producto != null)
                .GroupBy(m => new { m.IdProducto, Nombre = m.Producto!.Nombre })
                .Select(g => new TopProducto
                {
                    IdProducto = g.Key.IdProducto,
                    Nombre = g.Key.Nombre,
                    CantidadMovida = g.Sum(x => x.Cantidad),
                    IngresosEstimados = g.Sum(x => x.Cantidad * (x.Producto!.PrecioVenta - x.Producto!.PrecioCompra))
                })
                .OrderByDescending(t => t.CantidadMovida)
                .Take(10)
                .ToListAsync();

            ChartProductoLabels = TopProductos.Select(t => t.Nombre).ToList();
            ChartProductoValues = TopProductos.Select(t => t.CantidadMovida).ToList();

            var inicio = Desde?.Date ?? DateTime.Today.AddDays(-30);
            var fin = (Hasta?.Date ?? DateTime.Today).AddDays(1).AddTicks(-1);

            Ingresos = await _context.Pagos
                .Where(p => p.FechaHora >= inicio && p.FechaHora <= fin)
                .SumAsync(p => p.Monto);

            Egresos = await _context.DetEgresos
                .Join(_context.CabEgresos,
                      d => d.IdCabEgreso,
                      c => c.IdCabEgreso,
                      (d, c) => new { d.Monto, c.Fecha })
                .Where(x => x.Fecha >= inicio && x.Fecha <= fin)
                .SumAsync(x => x.Monto);

            TopServicios = await _context.DetallesServicio
                .Join(_context.Cuentas,
                      d => d.IdCuenta,
                      c => c.IdCuenta,
                      (d, c) => new { d, c.FechaHoraCreacion })
                .Where(x => x.FechaHoraCreacion >= inicio && x.FechaHoraCreacion <= fin)
                .GroupBy(x => x.d.IdServicio)
                .Select(g => new TopServicio
                {
                    IdServicio = g.Key,
                    Nombre = _context.Servicios.Where(s => s.IdServicio == g.Key).Select(s => s.Nombre).FirstOrDefault()!,
                    Cantidad = g.Sum(y => y.d.Cantidad),
                    Importe = g.Sum(y => y.d.Subtotal)
                })
                .OrderByDescending(t => t.Cantidad)
                .Take(10)
                .ToListAsync();

            ChartServicioLabels = TopServicios.Select(t => t.Nombre).ToList();
            ChartServicioValues = TopServicios.Select(t => t.Cantidad).ToList();

            // Series diaria ingresos/egresos (reales desde BD)
            var dias = Enumerable.Range(0, (fin.Date - inicio.Date).Days + 1)
                .Select(offset => inicio.Date.AddDays(offset)).ToList();
            ChartFechas = dias.Select(d => d.ToString("yyyy-MM-dd")).ToList();
            var ingresosPorDia = await _context.Pagos
                .Where(p => p.FechaHora >= inicio && p.FechaHora <= fin)
                .GroupBy(p => p.FechaHora.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(x => x.Monto) })
                .ToListAsync();
            var egresosPorDia = await _context.CabEgresos
                .Join(_context.DetEgresos,
                      c => c.IdCabEgreso,
                      d => d.IdCabEgreso,
                      (c, d) => new { c.Fecha, d.Monto })
                .Where(x => x.Fecha >= inicio && x.Fecha <= fin)
                .GroupBy(x => x.Fecha.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(x => x.Monto) })
                .ToListAsync();
            ChartIngresosDia = dias.Select(d => ingresosPorDia.FirstOrDefault(x => x.Fecha == d)?.Total ?? 0m).ToList();
            ChartEgresosDia = dias.Select(d => egresosPorDia.FirstOrDefault(x => x.Fecha == d)?.Total ?? 0m).ToList();
        }

        public async Task<IActionResult> OnGetRangoAsync()
        {
            await OnGet();
            return Partial("_RangoContentPartial", this);
        }

        public async Task<IActionResult> OnGetExportRangoAsync()
        {
            await OnGet();
            var sb = new StringBuilder();
            sb.Append("<html><head><meta charset='utf-8'><title>Reporte</title><style>body{font-family:Segoe UI,Arial;padding:20px;}h1{font-size:18px}table{width:100%;border-collapse:collapse}th,td{padding:8px;border:1px solid #e5e7eb}th{background:#f4f4f5;color:#6b7280;text-transform:uppercase;font-size:12px} .card{border:1px solid #e5e7eb;border-radius:8px;padding:12px;margin-bottom:12px}</style></head><body>");
            sb.Append("<h1>Reporte de Ingresos/Egresos y Top Servicios</h1>");
            sb.Append($"<div class='card'><div>Ingresos: S/ {Ingresos:N2}</div><div>Egresos: S/ {Egresos:N2}</div><div>Utilidad: S/ {Utilidad:N2}</div></div>");
            sb.Append("<div class='card'><table><thead><tr><th>Servicio</th><th>Cantidad</th><th>Importe</th></tr></thead><tbody>");
            if (TopServicios.Any())
            {
                foreach (var t in TopServicios)
                {
                    sb.Append($"<tr><td>{t.Nombre}</td><td style='text-align:right'>{t.Cantidad}</td><td style='text-align:right'>S/ {t.Importe:N2}</td></tr>");
                }
            }
            else
            {
                sb.Append("<tr><td colspan='3' style='text-align:center;color:#6b7280'>Sin servicios en el rango</td></tr>");
            }
            sb.Append("</tbody></table></div>");
            sb.Append("</body></html>");
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/html", $"reporte_{DateTime.Now:yyyyMMdd_HHmm}.html");
        }

        public async Task<IActionResult> OnGetChartDataAsync()
        {
            var inicio = Desde?.Date ?? DateTime.Today.AddDays(-30);
            var fin = (Hasta?.Date ?? DateTime.Today).AddDays(1).AddTicks(-1);

            var topProductos = await _context.MovimientoInventario
                .Include(m => m.Producto)
                .Where(m => m.Producto != null && m.Fecha >= inicio && m.Fecha <= fin)
                .GroupBy(m => new { m.IdProducto, Nombre = m.Producto!.Nombre })
                .Select(g => new { nombre = g.Key.Nombre, cantidad = g.Sum(x => x.Cantidad) })
                .OrderByDescending(t => t.cantidad)
                .Take(10)
                .ToListAsync();

            var topServicios = await _context.DetallesServicio
                .Join(_context.Cuentas, d => d.IdCuenta, c => c.IdCuenta, (d, c) => new { d, c.FechaHoraCreacion })
                .Where(x => x.FechaHoraCreacion >= inicio && x.FechaHoraCreacion <= fin)
                .GroupBy(x => x.d.IdServicio)
                .Select(g => new
                {
                    nombre = _context.Servicios.Where(s => s.IdServicio == g.Key).Select(s => s.Nombre).FirstOrDefault()!,
                    cantidad = g.Sum(y => y.d.Cantidad)
                })
                .OrderByDescending(t => t.cantidad)
                .Take(10)
                .ToListAsync();

            var payload = new
            {
                productoLabels = topProductos.Select(t => t.nombre).ToList(),
                productoValues = topProductos.Select(t => t.cantidad).ToList(),
                servicioLabels = topServicios.Select(t => t.nombre).ToList(),
                servicioValues = topServicios.Select(t => t.cantidad).ToList()
            };
            return new JsonResult(payload);
        }

        public async Task<IActionResult> OnGetDataAsync()
        {
            var inicio = Desde?.Date ?? DateTime.Today.AddDays(-30);
            var fin = (Hasta?.Date ?? DateTime.Today).AddDays(1).AddTicks(-1);

            var ingresos = await _context.Pagos
                .Where(p => p.FechaHora >= inicio && p.FechaHora <= fin)
                .SumAsync(p => p.Monto);

            var egresos = await _context.CabEgresos
                .Join(_context.DetEgresos, c => c.IdCabEgreso, d => d.IdCabEgreso, (c, d) => new { c.Fecha, d.Monto })
                .Where(x => x.Fecha >= inicio && x.Fecha <= fin)
                .SumAsync(x => x.Monto);

            var topServicios = await _context.DetallesServicio
                .Join(_context.Cuentas, d => d.IdCuenta, c => c.IdCuenta, (d, c) => new { d, c.FechaHoraCreacion })
                .Where(x => x.FechaHoraCreacion >= inicio && x.FechaHoraCreacion <= fin)
                .GroupBy(x => x.d.IdServicio)
                .Select(g => new
                {
                    nombre = _context.Servicios.Where(s => s.IdServicio == g.Key).Select(s => s.Nombre).FirstOrDefault()!,
                    cantidad = g.Sum(y => y.d.Cantidad),
                    importe = g.Sum(y => y.d.Subtotal)
                })
                .OrderByDescending(t => t.cantidad)
                .Take(10)
                .ToListAsync();

            var payload = new
            {
                finanzas = new { ingresos, egresos, utilidad = ingresos - egresos },
                topServicios
            };
            return new JsonResult(payload);
        }

        public async Task<IActionResult> OnGetExportInventarioAsync()
        {
            var productos = await _context.Productos.Include(p => p.CategoriaProducto).OrderBy(p => p.Nombre).ToListAsync();
            var sb = new StringBuilder();
            sb.AppendLine("Codigo,Nombre,Categoria,PrecioCompra,PrecioVenta,StockActual,StockMinimo");
            foreach (var p in productos)
            {
                sb.AppendLine($"{p.Codigo},{p.Nombre},{p.CategoriaProducto?.Nombre ?? ""},{p.PrecioCompra},{p.PrecioVenta},{p.StockActual},{p.StockMinimo}");
            }
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "inventario.csv");
        }

        public class TopProducto
        {
            public int IdProducto { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public int CantidadMovida { get; set; }
            public decimal IngresosEstimados { get; set; }
        }

        public class TopServicio
        {
            public int IdServicio { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public int Cantidad { get; set; }
            public decimal Importe { get; set; }
        }

        public async Task<IActionResult> OnGetExportPdfAsync()
        {
            var inicio = Desde?.Date ?? DateTime.Today.AddDays(-30);
            var fin = (Hasta?.Date ?? DateTime.Today).AddDays(1).AddTicks(-1);

            var ingresos = await _context.Pagos.Where(p => p.FechaHora >= inicio && p.FechaHora <= fin).SumAsync(p => p.Monto);
            var egresos = await _context.CabEgresos
                .Join(_context.DetEgresos, c => c.IdCabEgreso, d => d.IdCabEgreso, (c, d) => new { c.Fecha, d.Monto })
                .Where(x => x.Fecha >= inicio && x.Fecha <= fin)
                .SumAsync(x => x.Monto);
            var topServicios = await _context.DetallesServicio
                .Join(_context.Cuentas, d => d.IdCuenta, c => c.IdCuenta, (d, c) => new { d, c.FechaHoraCreacion })
                .Where(x => x.FechaHoraCreacion >= inicio && x.FechaHoraCreacion <= fin)
                .GroupBy(x => x.d.IdServicio)
                .Select(g => new
                {
                    nombre = _context.Servicios.Where(s => s.IdServicio == g.Key).Select(s => s.Nombre).FirstOrDefault()!,
                    cantidad = g.Sum(y => y.d.Cantidad),
                    importe = g.Sum(y => y.d.Subtotal)
                })
                .OrderByDescending(t => t.cantidad)
                .Take(20)
                .ToListAsync();

            var html = new StringBuilder();
            html.Append("<html><head><meta charset='utf-8'><title>Reporte PDF</title><style>@page{size:A4 portrait;margin:12mm;}body{font-family:Segoe UI,Arial;padding:0 8mm;}h1{font-size:18px;margin:0 0 8px}table{width:100%;border-collapse:collapse}th,td{padding:8px;border:1px solid #e5e7eb}th{background:#f4f4f5;color:#6b7280;text-transform:uppercase;font-size:12px} .card{border:1px solid #e5e7eb;border-radius:8px;padding:12px;margin-bottom:12px}</style></head><body>");
            html.Append($"<h1>Reporte (Rango: {inicio:yyyy-MM-dd} a {fin:yyyy-MM-dd})</h1>");
            html.Append($"<div class='card'><strong>Ingresos:</strong> S/ {ingresos:N2} &nbsp;&nbsp; <strong>Egresos:</strong> S/ {egresos:N2} &nbsp;&nbsp; <strong>Utilidad:</strong> S/ {(ingresos - egresos):N2}</div>");
            html.Append("<div class='card'><table><thead><tr><th>Servicio</th><th>Cantidad</th><th>Importe</th></tr></thead><tbody>");
            if (topServicios.Any())
            {
                foreach (var t in topServicios)
                {
                    html.Append($"<tr><td>{t.nombre}</td><td style='text-align:right'>{t.cantidad}</td><td style='text-align:right'>S/ {t.importe:N2}</td></tr>");
                }
            }
            else
            {
                html.Append("<tr><td colspan='3' style='text-align:center;color:#6b7280'>Sin servicios en el rango</td></tr>");
            }
            html.Append("</tbody></table></div>");
            html.Append("<script>setTimeout(function(){window.print()},100);</script></body></html>");
            var bytes = Encoding.UTF8.GetBytes(html.ToString());
            return File(bytes, "text/html", $"reporte_{DateTime.Now:yyyyMMdd_HHmm}.pdf.html");
        }
    }
}
