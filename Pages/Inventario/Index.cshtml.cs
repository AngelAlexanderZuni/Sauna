using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;
using ProyectoSaunaKalixto.Web.Domain.Repositories;
using System.Linq;
using System.Security.Claims; 

namespace ProyectoSaunaKalixto.Web.Pages.Inventario
{
    [Authorize(Roles = "Administrador,Admin")]
    public class IndexModel : PageModel
    {
        public string VistaActual { get; set; } = "productos";
        private readonly SaunaDbContext _context;
        private readonly IProductoRepository _productosRepo;
        private readonly ICategoriaProductoRepository _categoriasRepo;
        private readonly IMovimientoInventarioRepository _movimientosRepo;
        private readonly ITipoMovimientoRepository _tiposRepo;

        public IndexModel(SaunaDbContext context,
                          IProductoRepository productosRepo,
                          ICategoriaProductoRepository categoriasRepo,
                          IMovimientoInventarioRepository movimientosRepo,
                          ITipoMovimientoRepository tiposRepo)
        {
            _context = context;
            _productosRepo = productosRepo;
            _categoriasRepo = categoriasRepo;
            _movimientosRepo = movimientosRepo;
            _tiposRepo = tiposRepo;
        }

        #region Propiedades de Vista

        public List<Producto> Productos { get; set; } = new();
        public List<CategoriaProducto> Categorias { get; set; } = new();
        public List<MovimientoInventario> Movimientos { get; set; } = new();
        public List<Producto> ProductosSeleccion { get; set; } = new();
        
        // Estadísticas del Dashboard
        public EstadisticasInventario Estadisticas { get; set; } = new();

        // Propiedades para mantener el estado del modal
        [BindProperty]
        public string? ModalToShow { get; set; }
        
        [BindProperty]
        public int? ProductoIdToEdit { get; set; }

        #endregion

        #region Propiedades de Filtros

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoriaId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EstadoStock { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowInactive { get; set; } = false;

        #endregion

        #region Métodos de Carga de Datos

        public async Task OnGetAsync()
        {
            await CargarDatosAsync();
            VistaActual = Request.Query["vista"].FirstOrDefault() ?? "productos";
        }

        private async Task CargarDatosAsync()
        {
            try { Categorias = (await _categoriasRepo.GetAllOrderedAsync()).ToList(); } 
            catch (Exception ex) { Console.WriteLine($"Categorias: {ex.Message}"); Categorias = new(); }
            
            try { Productos = (await _productosRepo.SearchAsync(Busqueda, CategoriaId, EstadoStock, ShowInactive)).ToList(); } 
            catch (Exception ex) { Console.WriteLine($"Productos: {ex.Message}"); Productos = new(); }
            
            try { await CargarEstadisticasAsync(); } 
            catch (Exception ex) { Console.WriteLine($"Estadisticas: {ex.Message}"); Estadisticas = new(); }
            
            try { ProductosSeleccion = (await _productosRepo.GetActivosAsync()).ToList(); } 
            catch (Exception ex) { Console.WriteLine($"ProdSeleccion: {ex.Message}"); ProductosSeleccion = new(); }
            
            try 
            {
                Console.WriteLine("=== CARGANDO MOVIMIENTOS ===");
                Movimientos = await _context.MovimientoInventario
                    .Include(m => m.Producto)
                    .Include(m => m.TipoMovimiento)
                    .OrderByDescending(m => m.Fecha)
                    .ToListAsync();
                Console.WriteLine($"✅ Movimientos cargados: {Movimientos.Count}");
                
                // Debug: mostrar primeros 3 movimientos
                foreach (var mov in Movimientos.Take(3))
                {
                    Console.WriteLine($"   - ID: {mov.IdMovimiento}, Producto: {mov.Producto?.Nombre ?? "NULL"}, Tipo: {mov.TipoMovimiento?.Nombre ?? "NULL"}");
                }
            } 
            catch (Exception ex) 
            { 
                Console.WriteLine($"❌ ERROR Movimientos: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
                Movimientos = new(); 
            }
            
            TempData["MovimientosCount"] = Movimientos.Count;
            Console.WriteLine($"=== TOTAL MOVIMIENTOS: {Movimientos.Count} ===");
        }

        private async Task CargarEstadisticasAsync()
        {
            var productosActivos = await _context.Productos
                .Where(p => p.Activo)
                .ToListAsync();

            Estadisticas = new EstadisticasInventario
            {
                TotalProductos = productosActivos.Count,
                ProductosStockBajo = productosActivos.Count(p => p.StockActual <= p.StockMinimo && p.StockActual > 0),
                ProductosSinStock = productosActivos.Count(p => p.StockActual == 0),
                ProductosStockNormal = productosActivos.Count(p => p.StockActual > p.StockMinimo),
                ValorTotalInventario = productosActivos.Sum(p => p.StockActual * p.PrecioVenta),
                ValorTotalCosto = productosActivos.Sum(p => p.StockActual * p.PrecioCompra),
                UnidadesTotales = productosActivos.Sum(p => p.StockActual)
            };
        }

        #endregion

        #region CRUD - Producto

        public async Task<IActionResult> OnPostCrearAsync(
            string codigo, 
            string nombre, 
            string? descripcion,
            decimal precioCompra,
            decimal precioVenta,
            int stockActual,
            int stockMinimo,
            int? categoriaId)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
                {
                    TempData["Error"] = "El código y nombre son obligatorios";
                    TempData["ErrorModal"] = "crear";
                    await CargarDatosAsync();
                    ModalToShow = "crear";
                    return Page();
                }

                if (precioCompra < 0 || precioVenta < 0)
                {
                    TempData["Error"] = "Los precios no pueden ser negativos";
                    TempData["ErrorModal"] = "crear";
                    await CargarDatosAsync();
                    ModalToShow = "crear";
                    return Page();
                }

                if (stockActual < 0 || stockMinimo < 0)
                {
                    TempData["Error"] = "Los valores de stock no pueden ser negativos";
                    TempData["ErrorModal"] = "crear";
                    await CargarDatosAsync();
                    ModalToShow = "crear";
                    return Page();
                }

                // Validar código único
                var codigoExiste = await _context.Productos
                    .AnyAsync(p => p.Codigo.ToLower() == codigo.ToLower());
                
                if (codigoExiste)
                {
                    TempData["Error"] = "Ya existe un producto con ese código";
                    TempData["ErrorModal"] = "crear";
                    await CargarDatosAsync();
                    ModalToShow = "crear";
                    return Page();
                }

                var producto = new Producto
                {
                    Codigo = codigo.Trim().ToUpper(),
                    Nombre = nombre.Trim(),
                    Descripcion = descripcion?.Trim(),
                    PrecioCompra = precioCompra,
                    PrecioVenta = precioVenta,
                    StockActual = stockActual,
                    StockMinimo = stockMinimo,
                    IdCategoriaProducto = categoriaId > 0 ? categoriaId : null,
                    Activo = true
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Producto '{nombre}' creado exitosamente";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear el producto. Por favor, intente nuevamente";
                TempData["ErrorModal"] = "crear";
                Console.WriteLine($"Error al crear producto: {ex.Message}");
                await CargarDatosAsync();
                ModalToShow = "crear";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostActualizarAsync(
            int id,
            string codigo,
            string nombre,
            string? descripcion,
            decimal precioCompra,
            decimal precioVenta,
            int stockActual,
            int stockMinimo,
            int? categoriaId)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    TempData["Error"] = "Producto no encontrado";
                    TempData["ErrorModal"] = "editar";
                    await CargarDatosAsync();
                    ModalToShow = "editar";
                    ProductoIdToEdit = id;
                    return Page();
                }

                // Validaciones
                if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
                {
                    TempData["Error"] = "El código y nombre son obligatorios";
                    TempData["ErrorModal"] = "editar";
                    await CargarDatosAsync();
                    ModalToShow = "editar";
                    ProductoIdToEdit = id;
                    return Page();
                }

                if (precioCompra < 0 || precioVenta < 0)
                {
                    TempData["Error"] = "Los precios no pueden ser negativos";
                    TempData["ErrorModal"] = "editar";
                    await CargarDatosAsync();
                    ModalToShow = "editar";
                    ProductoIdToEdit = id;
                    return Page();
                }

                // Validar código único (excepto el mismo producto)
                var codigoExiste = await _context.Productos
                    .AnyAsync(p => p.Codigo.ToLower() == codigo.ToLower() && p.IdProducto != id);
                
                if (codigoExiste)
                {
                    TempData["Error"] = "Ya existe otro producto con ese código";
                    TempData["ErrorModal"] = "editar";
                    await CargarDatosAsync();
                    ModalToShow = "editar";
                    ProductoIdToEdit = id;
                    return Page();
                }

                producto.Codigo = codigo.Trim().ToUpper();
                producto.Nombre = nombre.Trim();
                producto.Descripcion = descripcion?.Trim();
                producto.PrecioCompra = precioCompra;
                producto.PrecioVenta = precioVenta;
                producto.StockActual = stockActual;
                producto.StockMinimo = stockMinimo;
                producto.IdCategoriaProducto = categoriaId > 0 ? categoriaId : null;

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Producto '{nombre}' actualizado exitosamente";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el producto. Por favor, intente nuevamente";
                TempData["ErrorModal"] = "editar";
                Console.WriteLine($"Error al actualizar producto: {ex.Message}");
                await CargarDatosAsync();
                ModalToShow = "editar";
                ProductoIdToEdit = id;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    TempData["Error"] = "Producto no encontrado";
                    return RedirectToPage();
                }

                producto.Activo = false;
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Producto '{producto.Nombre}' eliminado exitosamente";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el producto. Por favor, intente nuevamente";
                Console.WriteLine($"Error al eliminar producto: {ex.Message}");
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostToggleActivoAsync(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    return new JsonResult(new { success = false, message = "Producto no encontrado" });
                }
                producto.Activo = !producto.Activo;
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, activo = producto.Activo });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cambiar estado: {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al cambiar estado" });
            }
        }

        public async Task<IActionResult> OnGetToggleActivoAsync(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    return new JsonResult(new { success = false, message = "Producto no encontrado" });
                }
                producto.Activo = !producto.Activo;
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true, activo = producto.Activo });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cambiar estado (GET): {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al cambiar estado" });
            }
        }

        #endregion

        #region API Endpoints

        public async Task<IActionResult> OnGetFiltrarAsync(string? busqueda, int? categoriaId, string? estadoStock, bool showInactive = false)
        {
            try
            {
                var lista = await _productosRepo.SearchAsync(busqueda, categoriaId, estadoStock, showInactive);
                var productos = lista.Select(p => new
                {
                    idProducto = p.IdProducto,
                    codigo = p.Codigo,
                    nombre = p.Nombre,
                    descripcion = p.Descripcion,
                    precioCompra = p.PrecioCompra,
                    precioVenta = p.PrecioVenta,
                    stockActual = p.StockActual,
                    stockMinimo = p.StockMinimo,
                    categoriaNombre = p.CategoriaProducto != null ? p.CategoriaProducto.Nombre : "Sin categoría",
                    stockBajo = p.StockActual <= p.StockMinimo
                }).ToList();

                return new JsonResult(new { success = true, productos });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en filtro: {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al filtrar productos" });
            }
        }

        public async Task<IActionResult> OnGetObtenerProductoAsync(int id)
        {
            try
            {
                var producto = await _context.Productos
                    .Include(p => p.CategoriaProducto)
                    .FirstOrDefaultAsync(p => p.IdProducto == id);

                if (producto == null)
                {
                    return new JsonResult(new { success = false, message = "Producto no encontrado" });
                }

                return new JsonResult(new
                {
                    success = true,
                    producto = new
                    {
                        producto.IdProducto,
                        producto.Codigo,
                        producto.Nombre,
                        producto.Descripcion,
                        producto.PrecioCompra,
                        producto.PrecioVenta,
                        producto.StockActual,
                        producto.StockMinimo,
                        producto.IdCategoriaProducto
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener producto: {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al cargar el producto" });
            }
        }

        public async Task<IActionResult> OnGetBuscarProductosAsync(string? term, bool includeInactive = false, int limit = 10)
        {
            try
            {
                var lista = await _productosRepo.SearchAsync(term, null, null, includeInactive);
                var productos = lista
                    .Take(limit)
                    .Select(p => new
                    {
                        idProducto = p.IdProducto,
                        nombre = p.Nombre,
                        codigo = p.Codigo,
                        precioCompra = p.PrecioCompra,
                        precioVenta = p.PrecioVenta,
                        stockActual = p.StockActual
                    })
                    .ToList();

                return new JsonResult(new { success = true, productos });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar productos: {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al buscar productos" });
            }
        }

        public async Task<IActionResult> OnGetMovimientosProductoAsync(int id)
        {
            try
            {
                var movimientos = await _context.MovimientoInventario
                    .Include(m => m.TipoMovimiento)
                    .Where(m => m.IdProducto == id)
                    .OrderByDescending(m => m.Fecha)
                    .Select(m => new
                    {
                        m.IdMovimiento,
                        fecha = m.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        tipo = m.TipoMovimiento != null ? m.TipoMovimiento.Nombre : "Sin tipo",
                        m.Cantidad,
                        m.Observacion,
                        m.CostoTotal
                    })
                    .ToListAsync();

                return new JsonResult(new { success = true, movimientos });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar movimientos: {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al cargar movimientos" });
            }
        }

        public async Task<IActionResult> OnGetFiltrarMovimientosAsync(string? producto, DateTime? desde, DateTime? hasta)
        {
            try
            {
                var query = _context.MovimientoInventario
                    .Include(m => m.Producto)
                    .Include(m => m.TipoMovimiento)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(producto))
                {
                    var term = producto.Trim().ToLower();
                    query = query.Where(m => m.Producto != null && m.Producto.Nombre.ToLower().Contains(term));
                }

                if (desde.HasValue) query = query.Where(m => m.Fecha >= desde.Value);
                if (hasta.HasValue) query = query.Where(m => m.Fecha <= hasta.Value);

                var movimientos = await query
                    .OrderByDescending(m => m.Fecha)
                    .Select(m => new
                    {
                        m.IdMovimiento,
                        fecha = m.Fecha.ToString("dd/MM/yyyy HH:mm"),
                        tipo = m.TipoMovimiento != null ? m.TipoMovimiento.Nombre : "Sin tipo",
                        m.Cantidad,
                        m.Observacion,
                        m.CostoTotal,
                        producto = m.Producto != null ? m.Producto.Nombre : "Producto desconocido"
                    })
                    .ToListAsync();

                return new JsonResult(new { success = true, movimientos });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al filtrar movimientos: {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al filtrar movimientos" });
            }
        }

        #endregion

        #region Gestión de Movimientos

        public async Task<IActionResult> OnPostAjustarStockAsync(
            int id, 
            int cantidad, 
            string tipo,
            decimal? costoUnitario = null,
            string? observacion = null,
            DateTime? fecha = null)
        {
            try
            {
                Console.WriteLine("=== INICIO AJUSTAR STOCK ===");
                Console.WriteLine($"ID Producto: {id}");
                Console.WriteLine($"Cantidad: {cantidad}");
                Console.WriteLine($"Tipo: {tipo}");

                // 1. OBTENER USUARIO AUTENTICADO
                var nombreUsuario = User?.Identity?.Name;
                var usuarioActual = !string.IsNullOrWhiteSpace(nombreUsuario)
                    ? await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario)
                    : null;
                if (usuarioActual == null)
                {
                    Console.WriteLine("❌ ERROR: Usuario autenticado no encontrado");
                    TempData["Error"] = "Usuario no identificado. Inicie sesión nuevamente.";
                    return RedirectToPage();
                }
                int idUsuario = usuarioActual.IdUsuario;
                Console.WriteLine($"✅ Usando usuario autenticado: {usuarioActual.NombreUsuario} (ID: {idUsuario})");

        // 2. Validar producto
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            Console.WriteLine("❌ ERROR: Producto no encontrado");
            TempData["Error"] = "Producto no encontrado";
            return RedirectToPage();
        }

        Console.WriteLine($"✅ Producto encontrado: {producto.Nombre}");
        Console.WriteLine($"   Stock actual: {producto.StockActual}");

        // 3. Validar cantidad
        if (cantidad <= 0)
        {
            Console.WriteLine("❌ ERROR: Cantidad inválida");
            TempData["Error"] = "La cantidad debe ser mayor a cero";
            return RedirectToPage();
        }

        // 4. Determinar tipo de movimiento
        var tipoNormalizado = (tipo ?? "entrada").ToLower().Trim();
        var esEntrada = tipoNormalizado == "entrada" || tipoNormalizado == "aumentar";

        Console.WriteLine($"Tipo normalizado: '{tipoNormalizado}'");
        Console.WriteLine($"Es entrada: {esEntrada}");

        // 5. Validar stock suficiente para salidas
        if (!esEntrada && producto.StockActual < cantidad)
        {
            Console.WriteLine($"❌ ERROR: Stock insuficiente ({producto.StockActual} < {cantidad})");
            TempData["Error"] = $"Stock insuficiente. Stock actual: {producto.StockActual}";
            return RedirectToPage();
        }

                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () => {
                using var tx = await _context.Database.BeginTransactionAsync();
                var tipoMovimientoNombre = esEntrada ? "Entrada" : "Salida";
                Console.WriteLine($"Buscando/creando tipo de movimiento: '{tipoMovimientoNombre}'");
                var tipoMovimiento = await _tiposRepo.GetOrCreateAsync(tipoMovimientoNombre);

        // 7. Calcular nuevo stock
        var stockAnterior = producto.StockActual;
        if (esEntrada)
        {
            producto.StockActual += cantidad;
        }
        else
        {
            producto.StockActual -= cantidad;
        }

        Console.WriteLine($"Stock: {stockAnterior} → {producto.StockActual}");

        // 8. Calcular costo
        var unit = (costoUnitario.HasValue && costoUnitario.Value > 0) 
            ? costoUnitario.Value 
            : producto.PrecioCompra;
        
        Console.WriteLine($"Costo unitario: {unit}");
        Console.WriteLine($"Costo total: {unit * cantidad}");

        // 9. Crear movimiento
        var movimiento = new MovimientoInventario
        {
            IdProducto = producto.IdProducto,
            Cantidad = cantidad,
            Fecha = fecha ?? DateTime.Now,
            Observacion = string.IsNullOrWhiteSpace(observacion) 
                ? $"Ajuste de stock - {tipoMovimientoNombre}" 
                : observacion.Trim(),
            CostoUnitario = unit,
            CostoTotal = unit * cantidad,
            IdUsuario = idUsuario
        };
        if (tipoMovimiento.IdTipoMovimiento == 0)
        {
            movimiento.TipoMovimiento = tipoMovimiento;
        }
        else
        {
            movimiento.IdTipoMovimiento = tipoMovimiento.IdTipoMovimiento;
        }

        Console.WriteLine($"Creando movimiento con idUsuario: {idUsuario}");
                _context.MovimientoInventario.Add(movimiento);

                // 10. Guardar cambios (atomicidad)
                Console.WriteLine("Guardando cambios en la base de datos (transacción)...");
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                });

        Console.WriteLine("✅ Stock ajustado exitosamente");
        Console.WriteLine("=== FIN AJUSTAR STOCK ===");

        TempData["Success"] = $"Stock ajustado exitosamente. Nuevo stock: {producto.StockActual}";
        return RedirectToPage();
    }
    catch (DbUpdateException dbEx)
    {
        Console.WriteLine($"❌ ERROR DE BASE DE DATOS:");
        Console.WriteLine($"   Message: {dbEx.Message}");
        Console.WriteLine($"   InnerException: {dbEx.InnerException?.Message}");
        
        var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
        TempData["Error"] = $"Error de base de datos: {innerMsg}";
        return RedirectToPage();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERROR GENERAL:");
        Console.WriteLine($"   Message: {ex.Message}");
        
        TempData["Error"] = $"Error al ajustar el stock: {ex.Message}";
        return RedirectToPage();
    }
}

        public async Task<IActionResult> OnPostActualizarMovimientoAsync(
            int idProducto, 
            string observacion)
        {
            try
            {
                var movimiento = await _movimientosRepo.GetUltimoPorProductoAsync(idProducto);
                if (movimiento == null)
                {
                    TempData["Error"] = "No hay movimientos para este producto";
                    return RedirectToPage();
                }

                movimiento.Observacion = string.IsNullOrWhiteSpace(observacion)
                    ? null
                    : observacion.Trim();

                await _context.SaveChangesAsync();

                TempData["Success"] = "Observación actualizada exitosamente";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el movimiento. Por favor, intente nuevamente";
                Console.WriteLine($"Error al actualizar movimiento: {ex.Message}");
                return RedirectToPage();
            }
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostEditarMovimientoAsync(int idMovimiento, string? observacion)
        {
            try
            {
                var mov = await _context.MovimientoInventario.FirstOrDefaultAsync(m => m.IdMovimiento == idMovimiento);
                if (mov == null)
                    return new JsonResult(new { success = false, message = "Movimiento no encontrado" });

                mov.Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar movimiento: {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al editar movimiento" });
            }
        }

        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostEditarMovimientoCompletoAsync(int idMovimiento, int cantidad, string tipo, string? observacion)
        {
            try
            {
                var strategy = _context.Database.CreateExecutionStrategy();
                int stockNuevo = 0;
                await strategy.ExecuteAsync(async () => {
                    using var tx = await _context.Database.BeginTransactionAsync();

                    var mov = await _context.MovimientoInventario.FirstOrDefaultAsync(m => m.IdMovimiento == idMovimiento);
                    if (mov == null) throw new InvalidOperationException("Movimiento no encontrado");

                    var producto = await _context.Productos.FirstOrDefaultAsync(p => p.IdProducto == mov.IdProducto);
                    if (producto == null) throw new InvalidOperationException("Producto no encontrado");

                    var tipoActual = await _context.TiposMovimiento.FirstOrDefaultAsync(t => t.IdTipoMovimiento == mov.IdTipoMovimiento);
                    var esEntradaActual = (tipoActual?.Nombre ?? "").ToLower().Contains("entrada");

                    producto.StockActual += esEntradaActual ? -mov.Cantidad : mov.Cantidad;

                    var tipoNorm = (tipo ?? "").Trim().ToLower();
                    var nombreTipo = tipoNorm.Contains("salida") ? "Salida" : "Entrada";
                    var tipoNuevo = await _tiposRepo.GetOrCreateAsync(nombreTipo);

                    var nuevoEsEntrada = nombreTipo.ToLower().Contains("entrada");
                    var ajuste = nuevoEsEntrada ? cantidad : -cantidad;

                    var stockPropuesto = producto.StockActual + ajuste;
                    if (stockPropuesto < 0) throw new InvalidOperationException("Stock insuficiente para aplicar la modificación");

                    producto.StockActual = stockPropuesto;

                    if (tipoNuevo.IdTipoMovimiento == 0)
                    {
                        mov.TipoMovimiento = tipoNuevo;
                    }
                    else
                    {
                        mov.IdTipoMovimiento = tipoNuevo.IdTipoMovimiento;
                    }
                    mov.Cantidad = cantidad;
                    mov.Observacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
                    mov.CostoTotal = mov.CostoUnitario * cantidad;

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();
                    stockNuevo = producto.StockActual;
                });

                return new JsonResult(new { success = true, stockActual = stockNuevo });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar completamente movimiento: {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al editar movimiento" });
            }
        }

        #endregion
    }

    #region Clases de Soporte

    public class EstadisticasInventario
    {
        public int TotalProductos { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosSinStock { get; set; }
        public int ProductosStockNormal { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public decimal ValorTotalCosto { get; set; }
        public int UnidadesTotales { get; set; }
        
        public decimal MargenTotal => ValorTotalInventario - ValorTotalCosto;
        public decimal PorcentajeMargen => ValorTotalCosto > 0 
            ? Math.Round((MargenTotal / ValorTotalCosto) * 100, 2) 
            : 0;
    }

    #endregion
}
