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
        public DbSet<CabEgreso> CabEgresos { get; set; }
        public DbSet<CategoriaServicio> CategoriasServicio { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<DetalleServicio> DetallesServicio { get; set; }
        public DbSet<DetEgreso> DetEgresos { get; set; }
        public DbSet<EstadoCuenta> EstadosCuenta { get; set; }
        public DbSet<MetodoPago> MetodosPago { get; set; }
        public DbSet<TipoComprobante> TiposComprobante { get; set; }
        public DbSet<TipoEgreso> TiposEgreso { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<ProgramaFidelizacion> ProgramasFidelizacion { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuario");
                entity.HasKey(e => e.IdUsuario);
                entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
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
                
                // Ignorar propiedades NotMapped
                entity.Ignore(e => e.NombreCompleto);
                entity.Ignore(e => e.TipoMembresia);
                entity.Ignore(e => e.FechaInicioMembresia);
                entity.Ignore(e => e.FechaFinMembresia);
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

            // CabEgreso
            modelBuilder.Entity<CabEgreso>(entity =>
            {
                entity.ToTable("CabEgreso");
                entity.HasKey(e => e.IdCabEgreso);
                entity.Property(e => e.IdCabEgreso).HasColumnName("idCabEgreso");
                entity.Property(e => e.Fecha).HasColumnName("fecha");
                entity.Property(e => e.MontoTotal).HasColumnName("montoTotal").HasColumnType("decimal(18,2)");
                entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
                entity.HasOne(e => e.Usuario)
                      .WithMany()
                      .HasForeignKey(e => e.IdUsuario)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // CategoriaServicio
            modelBuilder.Entity<CategoriaServicio>(entity =>
            {
                entity.ToTable("CategoriaServicio");
                entity.HasKey(e => e.IdCategoriaServicio);
                entity.Property(e => e.IdCategoriaServicio).HasColumnName("idCategoriaServicio");
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100).HasColumnName("nombre");
                entity.Property(e => e.Activo).HasColumnName("activo").HasDefaultValue(true);
            });

            // Servicio
            modelBuilder.Entity<Servicio>(entity =>
            {
                entity.ToTable("Servicio");
                entity.HasKey(e => e.IdServicio);
                entity.Property(e => e.IdServicio).HasColumnName("idServicio");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Precio).HasColumnName("precio").HasColumnType("decimal(12,2)").IsRequired();
                entity.Property(e => e.DuracionEstimada).HasColumnName("duracionEstimada");
                entity.Property(e => e.Activo).HasColumnName("activo").IsRequired();
                entity.Property(e => e.IdCategoriaServicio).HasColumnName("idCategoriaServicio");
                entity.HasIndex(e => e.Nombre).IsUnique();
                entity.HasOne(e => e.CategoriaServicio)
                      .WithMany()
                      .HasForeignKey(e => e.IdCategoriaServicio)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Cuenta
            modelBuilder.Entity<Cuenta>(entity =>
            {
                entity.ToTable("Cuenta");
                entity.HasKey(e => e.IdCuenta);
                entity.Property(e => e.IdCuenta).HasColumnName("idCuenta");
                entity.Property(e => e.FechaHoraCreacion).HasColumnName("fechaHoraCreacion");
                entity.Property(e => e.FechaHoraSalida).HasColumnName("fechaHoraSalida");
                entity.Property(e => e.SubtotalConsumos).HasColumnName("subtotalConsumos").HasColumnType("decimal(18,2)");
                entity.Property(e => e.Descuento).HasColumnName("descuento").HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnName("total").HasColumnType("decimal(18,2)");
                entity.Property(e => e.IdEstadoCuenta).HasColumnName("idEstadoCuenta");
                entity.Property(e => e.IdUsuarioCreador).HasColumnName("idUsuarioCreador");
                entity.Property(e => e.IdCliente).HasColumnName("idCliente");
                entity.HasOne(e => e.UsuarioCreador)
                      .WithMany()
                      .HasForeignKey(e => e.IdUsuarioCreador)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Cliente)
                      .WithMany()
                      .HasForeignKey(e => e.IdCliente)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.EstadoCuenta)
                      .WithMany()
                      .HasForeignKey(e => e.IdEstadoCuenta)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Comprobante
            modelBuilder.Entity<Comprobante>(entity =>
            {
                entity.ToTable("Comprobante");
                entity.HasKey(e => e.IdComprobante);
                entity.Property(e => e.IdComprobante).HasColumnName("idComprobante");
                entity.Property(e => e.Serie).IsRequired().HasMaxLength(10).HasColumnName("serie");
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(15).HasColumnName("numero");
                entity.Property(e => e.FechaEmision).HasColumnName("fechaEmision");
                entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(18,2)");
                entity.Property(e => e.Igv).HasColumnName("igv").HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnName("total").HasColumnType("decimal(18,2)");
                entity.Property(e => e.IdTipoComprobante).HasColumnName("idTipoComprobante");
                entity.Property(e => e.IdCuenta).HasColumnName("idCuenta");
                entity.HasOne(e => e.Cuenta)
                      .WithMany(c => c.Comprobantes)
                      .HasForeignKey(e => e.IdCuenta)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.TipoComprobante)
                      .WithMany()
                      .HasForeignKey(e => e.IdTipoComprobante)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // DetalleServicio
            modelBuilder.Entity<DetalleServicio>(entity =>
            {
                entity.ToTable("DetalleServicio");
                entity.HasKey(e => e.IdDetalleServicio);
                entity.Property(e => e.IdDetalleServicio).HasColumnName("idDetalleServicio");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.PrecioUnitario).HasColumnName("precioUnitario").HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(18,2)");
                entity.Property(e => e.IdCuenta).HasColumnName("idCuenta");
                entity.Property(e => e.IdServicio).HasColumnName("idServicio");
                entity.HasOne(e => e.Cuenta)
                      .WithMany(c => c.DetallesServicio)
                      .HasForeignKey(e => e.IdCuenta)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Servicio)
                      .WithMany(s => s.DetallesServicio)
                      .HasForeignKey(e => e.IdServicio)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // DetEgreso
            modelBuilder.Entity<DetEgreso>(entity =>
            {
                entity.ToTable("DetEgreso");
                entity.HasKey(e => e.IdDetEgreso);
                entity.Property(e => e.IdDetEgreso).HasColumnName("idDetEgreso");
                entity.Property(e => e.IdCabEgreso).HasColumnName("idCabEgreso");
                entity.Property(e => e.Concepto).HasColumnName("concepto").HasMaxLength(200);
                entity.Property(e => e.Monto).HasColumnName("monto").HasColumnType("decimal(12,2)");
                entity.Property(e => e.Recurrente).HasColumnName("recurrente");
                entity.Property(e => e.ComprobanteRuta).HasColumnName("comprobanteRuta").HasMaxLength(80);
                entity.Property(e => e.IdTipoEgreso).HasColumnName("idTipoEgreso");
                entity.HasOne(e => e.CabEgreso)
                      .WithMany(c => c.DetallesEgreso)
                      .HasForeignKey(e => e.IdCabEgreso)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.TipoEgreso)
                      .WithMany()
                      .HasForeignKey(e => e.IdTipoEgreso)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // TipoComprobante
            modelBuilder.Entity<TipoComprobante>(entity =>
            {
                entity.ToTable("TipoComprobante");
                entity.HasKey(e => e.IdTipoComprobante);
                entity.Property(e => e.IdTipoComprobante).HasColumnName("idTipoComprobante");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(30).IsRequired();
            });

            // TipoEgreso
            modelBuilder.Entity<TipoEgreso>(entity =>
            {
                entity.ToTable("TipoEgreso");
                entity.HasKey(e => e.IdTipoEgreso);
                entity.Property(e => e.IdTipoEgreso).HasColumnName("idTipoEgreso");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            });

            // ProgramaFidelizacion
            modelBuilder.Entity<ProgramaFidelizacion>(entity =>
            {
                entity.ToTable("ProgramaFidelizacion");
                entity.HasKey(e => e.IdPrograma);
                entity.Property(e => e.IdPrograma).HasColumnName("idPrograma");
                entity.Property(e => e.VisitasParaDescuento).HasColumnName("visitasParaDescuento");
                entity.Property(e => e.PorcentajeDescuento).HasColumnName("porcentajeDescuento").HasColumnType("decimal(5,2)");
                entity.Property(e => e.DescuentoCumpleanos).HasColumnName("descuentoCumpleanos");
                entity.Property(e => e.MontoDescuentoCumpleanos).HasColumnName("montoDescuentoCumpleanos").HasColumnType("decimal(12,2)");
            });

            // EstadoCuenta
            modelBuilder.Entity<EstadoCuenta>(entity =>
            {
                entity.ToTable("EstadoCuenta");
                entity.HasKey(e => e.IdEstadoCuenta);
                entity.Property(e => e.IdEstadoCuenta).HasColumnName("idEstadoCuenta");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(30).IsRequired();
            });

            // MetodoPago
            modelBuilder.Entity<MetodoPago>(entity =>
            {
                entity.ToTable("MetodoPago");
                entity.HasKey(e => e.IdMetodoPago);
                entity.Property(e => e.IdMetodoPago).HasColumnName("idMetodoPago");
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
            });

            // Pago
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.ToTable("Pago");
                entity.HasKey(e => e.IdPago);
                entity.Property(e => e.IdPago).HasColumnName("idPago");
                entity.Property(e => e.FechaHora).HasColumnName("fechaHora");
                entity.Property(e => e.Monto).HasColumnName("monto").HasColumnType("decimal(12,2)");
                entity.Property(e => e.NumeroReferencia).HasColumnName("numeroReferencia").HasMaxLength(100);
                entity.Property(e => e.IdMetodoPago).HasColumnName("idMetodoPago");
                entity.Property(e => e.IdCuenta).HasColumnName("idCuenta");
                entity.HasOne(e => e.MetodoPago)
                      .WithMany()
                      .HasForeignKey(e => e.IdMetodoPago)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Cuenta)
                      .WithMany(c => c.Pagos)
                      .HasForeignKey(e => e.IdCuenta)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
