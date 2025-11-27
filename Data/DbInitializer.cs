using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Data
{
    public static class DbInitializer
    {
        public static void Run(SaunaDbContext context)
        {
            var sql = @"\nIF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TipoDescuento]') AND type in (N'U'))\nBEGIN\n    CREATE TABLE [dbo].[TipoDescuento] (\n        [idTipoDescuento] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,\n        [nombre] NVARCHAR(50) NOT NULL\n    );\nEND\n\nIF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Promociones]') AND type in (N'U'))\nBEGIN\n    CREATE TABLE [dbo].[Promociones] (\n        [idPromocion] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,\n        [nombreDescuento] NVARCHAR(100) NOT NULL,\n        [montoDescuento] DECIMAL(10,2) NOT NULL CONSTRAINT DF_Promociones_Monto DEFAULT(0),\n        [idTipoDescuento] INT NOT NULL,\n        [valorCondicion] INT NULL,\n        [activo] BIT NOT NULL CONSTRAINT DF_Promociones_Activo DEFAULT(1),\n        [motivo] NVARCHAR(200) NULL\n    );\nEND\n\nIF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Promociones_TipoDescuento')\nBEGIN\n    ALTER TABLE [dbo].[Promociones]\n    ADD CONSTRAINT [FK_Promociones_TipoDescuento] FOREIGN KEY ([idTipoDescuento]) REFERENCES [dbo].[TipoDescuento]([idTipoDescuento]) ON DELETE NO ACTION;\nEND\n\nIF COL_LENGTH('dbo.Cuenta','idPromocion') IS NULL\nBEGIN\n    ALTER TABLE [dbo].[Cuenta] ADD [idPromocion] INT NULL;\nEND\n\nIF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Cuenta_Promociones')\nBEGIN\n    ALTER TABLE [dbo].[Cuenta]\n    ADD CONSTRAINT [FK_Cuenta_Promociones] FOREIGN KEY ([idPromocion]) REFERENCES [dbo].[Promociones]([idPromocion]) ON DELETE SET NULL;\nEND\n";

            try
            {
                context.Database.ExecuteSqlRaw(sql);
            }
            catch
            {
                // silencio: puede fallar si ya existen o por permisos
            }

            // Asegurar creación general por modelo
            try
            {
                context.Database.EnsureCreated();
            }
            catch { }

            if (!context.TiposDescuento.Any())
            {
                context.TiposDescuento.AddRange(
                    new TipoDescuento { Nombre = "General" },
                    new TipoDescuento { Nombre = "Especial" }
                );
                context.SaveChanges();
            }
        }
    }
}
