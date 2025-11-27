using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Mappers;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;
using ProyectoSaunaKalixto.Web.Hubs;
using ProyectoSaunaKalixto.Web.Data;

namespace ProyectoSaunaKalixto.Web.Domain.Services
{
    public interface IDetalleConsumoService
    {
        Task<DetalleConsumoDto> CrearAsync(DetalleConsumoCreateDto dto);
        Task<DetalleConsumoDto?> ActualizarAsync(int id, DetalleConsumoUpdateDto dto);
        Task<bool> EliminarAsync(int id);
        Task<IEnumerable<DetalleConsumoDto>> ListarPorCuentaAsync(int idCuenta);
    }

    public class DetalleConsumoService : IDetalleConsumoService
    {
        private readonly IDetalleConsumoRepository _repo;
        private readonly IProductoRepository _productos;
        private readonly IMovimientoInventarioRepository _movs;
        private readonly ITipoMovimientoRepository _tipos;
        private readonly IUsuarioRepository _usuarios;
        private readonly IHubContext<InventarioHub> _hub;
        private readonly IHttpContextAccessor _http;
        private readonly SaunaDbContext _db;

        public DetalleConsumoService(
            IDetalleConsumoRepository repo,
            IProductoRepository productos,
            IMovimientoInventarioRepository movs,
            ITipoMovimientoRepository tipos,
            IUsuarioRepository usuarios,
            IHubContext<InventarioHub> hub,
            IHttpContextAccessor http,
            SaunaDbContext db
        ){ _repo = repo; _productos = productos; _movs = movs; _tipos = tipos; _usuarios = usuarios; _hub = hub; _http = http; _db = db; }

        public async Task<DetalleConsumoDto> CrearAsync(DetalleConsumoCreateDto dto)
        {
            Validar(dto.Cantidad, dto.PrecioUnitario);
            var entity = new DetalleConsumo
            {
                IdCuenta = dto.IdCuenta,
                IdProducto = dto.IdProducto,
                Cantidad = dto.Cantidad,
                PrecioUnitario = dto.PrecioUnitario,
                Subtotal = dto.Cantidad * dto.PrecioUnitario
            };
            var saved = await _repo.AddAsync(entity);

            var producto = await _productos.GetByIdAsync(dto.IdProducto);
            if (producto != null)
            {
                producto.StockActual = Math.Max(0, producto.StockActual - dto.Cantidad);
                await _productos.UpdateAsync(producto);

                var tipoSalida = await _db.TiposMovimiento.FirstOrDefaultAsync(t => t.Nombre == "Salida")
                                  ?? new TipoMovimiento { Nombre = "Salida" };
                if (tipoSalida.IdTipoMovimiento == 0){ _db.TiposMovimiento.Add(tipoSalida); await _db.SaveChangesAsync(); }

                var mov = new MovimientoInventario
                {
                    IdProducto = producto.IdProducto,
                    IdTipoMovimiento = tipoSalida.IdTipoMovimiento,
                    Cantidad = dto.Cantidad,
                    CostoUnitario = producto.PrecioCompra,
                    CostoTotal = producto.PrecioCompra * dto.Cantidad,
                    Fecha = DateTime.Now,
                    Observacion = $"Salida por consumo en cuenta #{dto.IdCuenta}",
                    IdUsuario = await ResolveUsuarioIdAsync()
                };
                await _movs.AddAsync(mov);
                await _hub.Clients.All.SendAsync("StockActualizado", producto.IdProducto, producto.StockActual);
                await _hub.Clients.All.SendAsync("MovimientoRegistrado", new { idMovimiento = mov.IdMovimiento, idProducto = mov.IdProducto, tipo = "Salida", cantidad = mov.Cantidad, fecha = mov.Fecha.ToString("yyyy-MM-dd HH:mm") });
            }

            return DetalleConsumoMapper.ToDto(saved);
        }

        public async Task<DetalleConsumoDto?> ActualizarAsync(int id, DetalleConsumoUpdateDto dto)
        {
            Validar(dto.Cantidad, dto.PrecioUnitario);
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return null;
            var oldCantidad = d.Cantidad;
            d.Cantidad = dto.Cantidad;
            d.PrecioUnitario = dto.PrecioUnitario;
            d.Subtotal = dto.Cantidad * dto.PrecioUnitario;
            await _repo.UpdateAsync(d);

            var producto = await _productos.GetByIdAsync(d.IdProducto);
            if (producto != null)
            {
                var diff = dto.Cantidad - oldCantidad;
                if (diff != 0)
                {
                    if (diff > 0)
                    {
                        producto.StockActual = Math.Max(0, producto.StockActual - diff);
                        var tipoSalida = await _db.TiposMovimiento.FirstOrDefaultAsync(t => t.Nombre == "Salida")
                                          ?? new TipoMovimiento { Nombre = "Salida" };
                        if (tipoSalida.IdTipoMovimiento == 0){ _db.TiposMovimiento.Add(tipoSalida); await _db.SaveChangesAsync(); }
                        var mov = new MovimientoInventario
                        {
                            IdProducto = producto.IdProducto,
                            IdTipoMovimiento = tipoSalida.IdTipoMovimiento,
                            Cantidad = diff,
                            CostoUnitario = producto.PrecioCompra,
                            CostoTotal = producto.PrecioCompra * diff,
                            Fecha = DateTime.Now,
                            Observacion = $"Ajuste por aumento de consumo en cuenta #{d.IdCuenta}",
                            IdUsuario = await ResolveUsuarioIdAsync()
                        };
                        await _productos.UpdateAsync(producto);
                        await _movs.AddAsync(mov);
                        await _hub.Clients.All.SendAsync("StockActualizado", producto.IdProducto, producto.StockActual);
                        await _hub.Clients.All.SendAsync("MovimientoRegistrado", new { idMovimiento = mov.IdMovimiento, idProducto = mov.IdProducto, tipo = "Salida", cantidad = mov.Cantidad, fecha = mov.Fecha.ToString("yyyy-MM-dd HH:mm") });
                    }
                    else
                    {
                        var entradaCant = -diff;
                        producto.StockActual += entradaCant;
                        var tipoEntrada = await _db.TiposMovimiento.FirstOrDefaultAsync(t => t.Nombre == "Entrada")
                                          ?? new TipoMovimiento { Nombre = "Entrada" };
                        if (tipoEntrada.IdTipoMovimiento == 0){ _db.TiposMovimiento.Add(tipoEntrada); await _db.SaveChangesAsync(); }
                        var mov = new MovimientoInventario
                        {
                            IdProducto = producto.IdProducto,
                            IdTipoMovimiento = tipoEntrada.IdTipoMovimiento,
                            Cantidad = entradaCant,
                            CostoUnitario = producto.PrecioCompra,
                            CostoTotal = producto.PrecioCompra * entradaCant,
                            Fecha = DateTime.Now,
                            Observacion = $"Ajuste por reducción de consumo en cuenta #{d.IdCuenta}",
                            IdUsuario = await ResolveUsuarioIdAsync()
                        };
                        await _productos.UpdateAsync(producto);
                        await _movs.AddAsync(mov);
                        await _hub.Clients.All.SendAsync("StockActualizado", producto.IdProducto, producto.StockActual);
                        await _hub.Clients.All.SendAsync("MovimientoRegistrado", new { idMovimiento = mov.IdMovimiento, idProducto = mov.IdProducto, tipo = "Entrada", cantidad = mov.Cantidad, fecha = mov.Fecha.ToString("yyyy-MM-dd HH:mm") });
                    }
                }
            }
            return DetalleConsumoMapper.ToDto(d);
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var d = await _repo.GetByIdAsync(id);
            if (d == null) return false;
            var idCuenta = d.IdCuenta;
            await _repo.DeleteAsync(id);

            var producto = await _productos.GetByIdAsync(d.IdProducto);
            if (producto != null)
            {
                producto.StockActual += d.Cantidad;
                await _productos.UpdateAsync(producto);

                var tipoEntrada = await _db.TiposMovimiento.FirstOrDefaultAsync(t => t.Nombre == "Entrada")
                                  ?? new TipoMovimiento { Nombre = "Entrada" };
                if (tipoEntrada.IdTipoMovimiento == 0){ _db.TiposMovimiento.Add(tipoEntrada); await _db.SaveChangesAsync(); }

                var mov = new MovimientoInventario
                {
                    IdProducto = producto.IdProducto,
                    IdTipoMovimiento = tipoEntrada.IdTipoMovimiento,
                    Cantidad = d.Cantidad,
                    CostoUnitario = producto.PrecioCompra,
                    CostoTotal = producto.PrecioCompra * d.Cantidad,
                    Fecha = DateTime.Now,
                    Observacion = $"Entrada por eliminación/devolución de consumo en cuenta #{idCuenta}",
                    IdUsuario = await ResolveUsuarioIdAsync()
                };
                await _movs.AddAsync(mov);
                await _hub.Clients.All.SendAsync("StockActualizado", producto.IdProducto, producto.StockActual);
                await _hub.Clients.All.SendAsync("MovimientoRegistrado", new { idMovimiento = mov.IdMovimiento, idProducto = mov.IdProducto, tipo = "Entrada", cantidad = mov.Cantidad, fecha = mov.Fecha.ToString("yyyy-MM-dd HH:mm") });
            }
            return true;
        }

        public async Task<IEnumerable<DetalleConsumoDto>> ListarPorCuentaAsync(int idCuenta)
        {
            var list = await _repo.GetByCuentaIdAsync(idCuenta);
            return list.Select(DetalleConsumoMapper.ToDto);
        }

        private static void Validar(int cantidad, decimal precio)
        {
            if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser > 0");
            if (precio < 0) throw new ArgumentException("Precio no puede ser negativo");
        }

        private async Task<int> ResolveUsuarioIdAsync()
        {
            try
            {
                var nombreUsuario = _http?.HttpContext?.User?.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(nombreUsuario))
                {
                    var u = await _usuarios.GetByIdAsync(nombreUsuario);
                    if (u != null) return u.IdUsuario;
                }
            }
            catch {}
            var fallback = await _db.Usuarios.OrderBy(u => u.IdUsuario).FirstOrDefaultAsync();
            return fallback?.IdUsuario ?? 1;
        }
    }
}

