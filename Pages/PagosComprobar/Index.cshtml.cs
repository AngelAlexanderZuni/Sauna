using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.PagosComprobar
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICuentaService _cuentas;
        private readonly IMetodoPagoService _metodos;
        private readonly ITipoComprobanteService _tipos;
        private readonly IPagoService _pagos;
        private readonly IComprobanteService _comprobantes;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;
        private readonly IDetalleServicioService _detServicios;
        private readonly IDetalleConsumoService _detConsumos;
        private readonly IClienteService _clientes;

        public IndexModel(ICuentaService cuentas, IMetodoPagoService metodos, ITipoComprobanteService tipos, IPagoService pagos, IComprobanteService comprobantes, Microsoft.Extensions.Configuration.IConfiguration config, IDetalleServicioService detServicios, IDetalleConsumoService detConsumos, IClienteService clientes)
        { _cuentas = cuentas; _metodos = metodos; _tipos = tipos; _pagos = pagos; _comprobantes = comprobantes; _config = config; _detServicios = detServicios; _detConsumos = detConsumos; _clientes = clientes; }

        [BindProperty(SupportsGet = true)]
        public int cuentaId { get; set; }

        public CuentaDto? Cuenta { get; set; }
        public List<MetodoPagoDto> Metodos { get; set; } = new();
        public List<TipoComprobanteDto> TiposComprobante { get; set; } = new();
        public List<PagoDto> Pagos { get; set; } = new();
        public List<ComprobanteDto> Comprobantes { get; set; } = new();
        public Dictionary<string,string> SeriesPorTipo { get; set; } = new();
        public List<DetalleServicioDto> ServiciosDetalle { get; set; } = new();
        public List<DetalleConsumoDto> ProductosDetalle { get; set; } = new();
        public decimal IgvRate { get; set; }
        public decimal BaseImponible { get; set; }
        public decimal IgvMonto { get; set; }
        public decimal DescuentoAplicado { get; set; }
        public decimal TotalMostrar { get; set; }
        public string ClienteNumeroDocumento { get; set; } = string.Empty;
        public string ClienteNombreCompleto { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            if (cuentaId <= 0)
            {
                var qId = HttpContext.Request.Query["cuentaId"].FirstOrDefault();
                if (int.TryParse(qId, out var parsed)) cuentaId = parsed;
            }

            Cuenta = await _cuentas.ObtenerAsync(cuentaId);
            var id = cuentaId > 0 ? cuentaId : (Cuenta?.IdCuenta ?? 0);
            Metodos = (await _metodos.ListarAsync()).ToList();
            TiposComprobante = (await _tipos.ListarAsync()).ToList();
            if (id > 0)
            {
                Pagos = (await _pagos.ListarPorCuentaAsync(id)).ToList();
                Comprobantes = (await _comprobantes.ListarPorCuentaAsync(id)).ToList();
                ServiciosDetalle = (await _detServicios.ListarPorCuentaAsync(id)).ToList();
                ProductosDetalle = (await _detConsumos.ListarPorCuentaAsync(id)).ToList();
            }
            else
            {
                Pagos = (await _pagos.ListarRecientesAsync(20)).ToList();
                Comprobantes = (await _comprobantes.ListarRecientesAsync(20)).ToList();
            }
            foreach(var t in TiposComprobante){
                var serieCfg = _config.GetValue<string>($"Comprobantes:SeriesPorTipo:{t.Nombre}", "B001");
                SeriesPorTipo[t.Nombre] = serieCfg;
            }
            IgvRate = _config.GetValue<decimal>("Comprobantes:IGV", 0.18m);
            var subServicios = Cuenta?.SubtotalServicios ?? 0m;
            var subProductos = Cuenta?.SubtotalProductos ?? 0m;
            var descuento = Cuenta?.PromocionMontoDescuento ?? Cuenta?.Descuento ?? 0m;
            BaseImponible = Math.Round(subServicios + subProductos - descuento, 2);
            IgvMonto = Math.Round(BaseImponible * IgvRate, 2);
            TotalMostrar = Math.Round(BaseImponible + IgvMonto, 2);
            DescuentoAplicado = Cuenta?.PromocionMontoDescuento ?? Cuenta?.Descuento ?? 0m;
            var clienteDto = Cuenta != null ? await _clientes.ObtenerAsync(Cuenta.IdCliente) : null;
            ClienteNumeroDocumento = clienteDto?.NumeroDocumento ?? string.Empty;
            ClienteNombreCompleto = (clienteDto != null) ? (clienteDto.Nombre + " " + clienteDto.Apellido).Trim() : (Cuenta?.ClienteNombre ?? string.Empty);
        }

        public async Task<IActionResult> OnPostRegistrarPagoComprobanteAsync(int idCuenta, int idMetodoPago, decimal monto, string? referencia, int idTipoComprobante, string serie)
        {
            try
            {
                var cuenta = await _cuentas.ObtenerAsync(idCuenta);
                if (cuenta == null) return new JsonResult(new { success = false, message = "Cuenta no encontrada" });

                var igvRate = _config.GetValue<decimal>("Comprobantes:IGV", 0.18m);
                var baseImp = Math.Round((cuenta.SubtotalServicios) + (cuenta.SubtotalProductos) - (cuenta.PromocionMontoDescuento ?? cuenta.Descuento), 2);
                var igv = Math.Round(baseImp * igvRate, 2);
                var totalConIgv = Math.Round(baseImp + igv, 2);
                if (Math.Round(monto,2) != totalConIgv)
                {
                    return new JsonResult(new { success = false, message = "El monto a pagar debe ser igual al Total con IGV" });
                }

                var pago = await _pagos.RegistrarAsync(new PagoCreateDto { IdCuenta = idCuenta, IdMetodoPago = idMetodoPago, Monto = monto, NumeroReferencia = referencia });

                var total = totalConIgv;
                var subtotal = baseImp;

                if(string.IsNullOrWhiteSpace(serie))
                {
                    var tipo = (await _tipos.ListarAsync()).FirstOrDefault(t=>t.IdTipoComprobante==idTipoComprobante);
                    var serieDefault = _config.GetValue<string>($"Comprobantes:SeriesPorTipo:{tipo?.Nombre}", "F001");
                    serie = serieDefault;
                }

                var comp = await _comprobantes.EmitirAsync(new ComprobanteCreateDto
                {
                    IdCuenta = idCuenta,
                    IdTipoComprobante = idTipoComprobante,
                    Serie = serie,
                    Subtotal = subtotal,
                    Igv = igv,
                    Total = total
                });

                await _cuentas.CerrarAsync(idCuenta, DateTime.Now);

                return new JsonResult(new { success = true, pago, comprobante = comp });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
    }
}
