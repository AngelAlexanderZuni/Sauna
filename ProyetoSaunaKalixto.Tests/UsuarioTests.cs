using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;

namespace ProyectoSaunaKalixto.Tests
{
    public class UsuarioTests
    {
        private SaunaDbContext NewContext()
        {
            var options = new DbContextOptionsBuilder<SaunaDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new SaunaDbContext(options);
        }

        [Fact]
        public async Task CrearUsuario_ExisteYSeObtienePorNombre()
        {
            using var ctx = NewContext();
            // Datos base de rol
            ctx.Roles.Add(new Rol { IdRol = 1, Nombre = "Administrador" });
            await ctx.SaveChangesAsync();

            var repo = new UsuarioRepository(ctx);
            var usuario = new Usuario
            {
                NombreUsuario = "admin_test",
                ContraseniaHash = "hash",
                Correo = "admin@test.com",
                IdRol = 1,
                Activo = true
            };

            await repo.AddAsync(usuario);

            Assert.True(await repo.ExistsAsync("admin_test"));
            var encontrado = await repo.GetByIdAsync("admin_test");
            Assert.NotNull(encontrado);
            Assert.Equal("admin_test", encontrado!.NombreUsuario);
        }
    }
}