using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Data
{
    public class SaunaDbContext : DbContext
    {
        public SaunaDbContext(DbContextOptions<SaunaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<CategoriaProducto> CategoriasProducto { get; set; }
        public DbSet<DetalleConsumo> DetallesConsumo { get; set; }
        public DbSet<TipoMovimiento> TiposMovimiento { get; set; }
        public DbSet<MovimientoInventario> MovimientoInventario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(e => e.NombreUsuario);
                entity.Property(e => e.NombreUsuario).IsRequired().HasMaxLength(50).HasColumnName("nombreUsuario");
                entity.Property(e => e.ContraseniaHash).IsRequired().HasColumnName("contraseniaHash");
                entity.Property(e => e.IdRol).HasColumnName("idRol");
                entity.Property(e => e.Correo).HasMaxLength(150).HasColumnName("correo");
                entity.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");

                entity.HasIndex(e => e.NombreUsuario).IsUnique();
            });

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.ClienteID);
                entity.Property(e => e.NumeroDocumento).HasMaxLength(50);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefono).HasMaxLength(50);
                entity.Property(e => e.Correo).HasMaxLength(100);
                entity.Property(e => e.Direccion).HasMaxLength(200);
                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.NumeroDocumento).IsUnique();
            });

            // Configuración de Rol
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Rol");
                entity.HasKey(e => e.IdRol);
                entity.Property(e => e.IdRol).HasColumnName("idRol");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50).HasColumnName("nombre");
            });

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Producto");
                entity.HasKey(e => e.IdProducto);
                entity.Property(e => e.IdProducto).HasColumnName("idProducto");
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20).HasColumnName("codigo");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).HasColumnName("nombre");
                entity.Property(e => e.Descripcion).HasMaxLength(500).HasColumnName("descripcion");
                entity.Property(e => e.PrecioCompra).HasColumnType("decimal(18,2)").HasColumnName("precioCompra");
                entity.Property(e => e.PrecioVenta).HasColumnType("decimal(18,2)").HasColumnName("precioVenta");
                entity.Property(e => e.StockActual).HasColumnName("stockActual");
                entity.Property(e => e.StockMinimo).HasColumnName("stockMinimo");
                entity.Property(e => e.IdCategoriaProducto).HasColumnName("idCategoriaProducto");
                entity.Property(e => e.Activo).HasDefaultValue(true).HasColumnName("activo");
                entity.Ignore(e => e.FechaRegistro);

                entity.HasIndex(e => e.Codigo).IsUnique();

                entity.HasOne(e => e.CategoriaProducto)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(e => e.IdCategoriaProducto)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de CategoriaProducto
            modelBuilder.Entity<CategoriaProducto>(entity =>
            {
                entity.ToTable("CategoriaProducto");
                entity.HasKey(e => e.IdCategoriaProducto);
                entity.Property(e => e.IdCategoriaProducto).HasColumnName("idCategoriaProducto");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).HasColumnName("nombre");
                entity.Ignore(e => e.Descripcion);
                entity.Ignore(e => e.Activo);
            });

            // Configuración de DetalleConsumo
            modelBuilder.Entity<DetalleConsumo>(entity =>
            {
                entity.ToTable("DetalleConsumo");
                entity.HasKey(e => e.IdDetalle);
                entity.Property(e => e.IdDetalle).HasColumnName("idDetalle");
                entity.Property(e => e.Cantidad).IsRequired().HasColumnName("cantidad");
                entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)").HasColumnName("precioUnitario");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)").HasColumnName("subtotal");
                entity.Property(e => e.IdOrden).HasColumnName("idOrden");
                entity.Property(e => e.IdProducto).HasColumnName("idProducto");

                entity.HasOne(e => e.Producto)
                    .WithMany(p => p.DetallesConsumo)
                    .HasForeignKey(e => e.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TipoMovimiento>(entity =>
            {
                entity.ToTable("TipoMovimiento");
                entity.HasKey(e => e.IdTipoMovimiento);
                entity.Property(e => e.IdTipoMovimiento).HasColumnName("idTipoMovimiento");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).HasColumnName("tipo");
            });

            modelBuilder.Entity<MovimientoInventario>(entity =>
            {
                entity.ToTable("MovimientoInventario");
                entity.HasKey(e => e.IdMovimiento);
                entity.Property(e => e.IdMovimiento).HasColumnName("idMovimiento");
                entity.Property(e => e.IdProducto).HasColumnName("idProducto");
                entity.Property(e => e.IdTipoMovimiento).HasColumnName("idTipoMovimiento");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.Fecha).HasColumnName("fecha");
                entity.Property(e => e.CostoUnitario).HasColumnName("costoUnitario").HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostoTotal).HasColumnName("costoTotal").HasColumnType("decimal(18,2)");
                entity.Property(e => e.Observacion).HasMaxLength(300).HasColumnName("observaciones");
                entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
                entity.HasOne(e => e.Producto)
                    .WithMany()
                    .HasForeignKey(e => e.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TipoMovimiento)
                    .WithMany(t => t.Movimientos)
                    .HasForeignKey(e => e.IdTipoMovimiento)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
