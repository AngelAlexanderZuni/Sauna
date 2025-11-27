using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;
using ProyectoSaunaKalixto.Web.Pages.Inventario;

namespace ProyectoSaunaKalixto.Tests
{
    public class InventoryTests
    {

        private SaunaDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<SaunaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SaunaDbContext(options);
        }

        private IndexModel NewIndexModel(SaunaDbContext ctx)
        {
            var repoProd = new ProductoRepository(ctx);
            var repoCat = new CategoriaProductoRepository(ctx);
            var repoMov = new MovimientoInventarioRepository(ctx);
            var repoTipo = new TipoMovimientoRepository(ctx);
            return new IndexModel(ctx, repoProd, repoCat, repoMov, repoTipo);
        }

        private static bool SuccessFrom(JsonResult? res)
        {
            if (res == null) return false;
            var json = System.Text.Json.JsonSerializer.Serialize(res.Value);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("success", out var s) && s.GetBoolean();
        }

        [Fact]
        public async Task ToggleActivo_Toggles_State()
        {
            using var ctx = NewContext();
            ctx.Productos.Add(new Producto { IdProducto = 1, Codigo = "P1", Nombre = "Prod", PrecioCompra = 10, PrecioVenta = 20, StockActual = 5, Activo = true });
            await ctx.SaveChangesAsync();
            var page = NewIndexModel(ctx);

            var res = await page.OnPostToggleActivoAsync(1) as Microsoft.AspNetCore.Mvc.JsonResult;
            Assert.True(SuccessFrom(res));
            var prod = await ctx.Productos.FindAsync(1);
            Assert.False(prod!.Activo);
        }

        [Fact]
        public async Task AjustarStock_Entrada_IncreasesAndRegistersMovement()
        {
            using var ctx = NewContext();
            ctx.Productos.Add(new Producto { IdProducto = 1, Codigo = "P1", Nombre = "Prod", PrecioCompra = 10, PrecioVenta = 20, StockActual = 5, Activo = true });
            ctx.TiposMovimiento.Add(new TipoMovimiento { IdTipoMovimiento = 1, Nombre = "Entrada" });
            await ctx.SaveChangesAsync();
            var page = NewIndexModel(ctx);

            var action = await page.OnPostAjustarStockAsync(1, 3, "entrada", null, "Ingreso", DateTime.UtcNow);
            var prod = await ctx.Productos.FindAsync(1);
            Assert.Equal(8, prod!.StockActual);
            var mov = await ctx.MovimientoInventario.OrderByDescending(m => m.IdMovimiento).FirstOrDefaultAsync();
            Assert.NotNull(mov);
            Assert.Equal(1, mov!.IdProducto);
            Assert.Equal(3, mov.Cantidad);
            Assert.Equal("Ingreso", mov.Observacion);
        }

        [Fact]
        public async Task AjustarStock_Salida_BlocksNegative()
        {
            using var ctx = NewContext();
            ctx.Productos.Add(new Producto { IdProducto = 1, Codigo = "P1", Nombre = "Prod", PrecioCompra = 10, PrecioVenta = 20, StockActual = 2, Activo = true });
            ctx.TiposMovimiento.Add(new TipoMovimiento { IdTipoMovimiento = 2, Nombre = "Salida" });
            await ctx.SaveChangesAsync();
            var page = NewIndexModel(ctx);

            var action = await page.OnPostAjustarStockAsync(1, 5, "salida", null, "Consumo", DateTime.UtcNow);
            var prod = await ctx.Productos.FindAsync(1);
            Assert.Equal(2, prod!.StockActual); // no cambia
        }

        [Fact]
        public async Task EditarMovimientoCompleto_RevertsAndAppliesNew()
        {
            using var ctx = NewContext();
            var prod = new Producto { IdProducto = 1, Codigo = "P1", Nombre = "Prod", PrecioCompra = 10, PrecioVenta = 20, StockActual = 10, Activo = true };
            ctx.Productos.Add(prod);
            var tipoEnt = new TipoMovimiento { IdTipoMovimiento = 1, Nombre = "Entrada" };
            ctx.TiposMovimiento.Add(tipoEnt);
            await ctx.SaveChangesAsync();

            var mov = new MovimientoInventario { IdProducto = 1, IdTipoMovimiento = 1, Cantidad = 4, CostoUnitario = 10, CostoTotal = 40, Fecha = DateTime.UtcNow };
            ctx.MovimientoInventario.Add(mov);
            await ctx.SaveChangesAsync();
            prod.StockActual += 4; // reflejar movimiento inicial
            await ctx.SaveChangesAsync();

            var page = NewIndexModel(ctx);
            var res = await page.OnPostEditarMovimientoCompletoAsync(mov.IdMovimiento, 2, "salida", "Obs") as Microsoft.AspNetCore.Mvc.JsonResult;
            Assert.True(SuccessFrom(res));
            var prodReload = await ctx.Productos.FindAsync(1);
            Assert.Equal(8, prodReload!.StockActual);
        }
    }
}
