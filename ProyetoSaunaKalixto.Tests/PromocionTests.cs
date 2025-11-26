using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Repositories;
using ProyectoSaunaKalixto.Web.Domain.Services;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using Xunit;

namespace ProyetoSaunaKalixto.Tests
{
    public class PromocionTests
    {
        private SaunaDbContext NewContext()
        {
            var opts = new DbContextOptionsBuilder<SaunaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SaunaDbContext(opts);
        }

        [Fact]
        public async Task CrearYListarPromocion()
        {
            using var ctx = NewContext();
            var promRepo = new PromocionRepository(ctx);
            var tipoRepo = new TipoDescuentoRepository(ctx);
            // seed tipo
            await tipoRepo.AddAsync(new ProyectoSaunaKalixto.Web.Domain.Models.TipoDescuento { Nombre = "General", Modo = "porcentaje" });
            var svc = new PromocionService(promRepo, tipoRepo);

            var tipos = await svc.TiposAsync();
            var dto = new PromocionCreateDto { Nombre = "Promo Test", Valor = 10, IdTipoDescuento = tipos.First().IdTipoDescuento };
            var created = await svc.CrearAsync(dto);
            var list = await svc.ListarAsync(null);
            Assert.Contains(list, p => p.Nombre == "Promo Test");
        }
    }
}

