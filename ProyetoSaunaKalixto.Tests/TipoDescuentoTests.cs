using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Repositories;
using ProyectoSaunaKalixto.Web.Domain.Services;
using Xunit;

namespace ProyetoSaunaKalixto.Tests
{
    public class TipoDescuentoTests
    {
        private SaunaDbContext NewContext()
        {
            var opts = new DbContextOptionsBuilder<SaunaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SaunaDbContext(opts);
        }

        [Fact]
        public async Task CrearYListarTipos()
        {
            using var ctx = NewContext();
            var promRepo = new PromocionRepository(ctx);
            var tipoRepo = new TipoDescuentoRepository(ctx);
            var svc = new PromocionService(promRepo, tipoRepo);

            var t1 = await svc.CrearTipoAsync("General");
            var t2 = await svc.CrearTipoAsync("Familia");
            var list = await svc.TiposAsync();
            Assert.Contains(list, t => t.Nombre == "General");
            Assert.Contains(list, t => t.Nombre == "Familia");
        }
    }
}

