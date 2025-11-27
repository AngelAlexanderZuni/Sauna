using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Repositories;
using ProyectoSaunaKalixto.Web.Domain.Services;
using ProyectoSaunaKalixto.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        // Configurar para aceptar tanto camelCase como PascalCase
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// Agregar soporte para API Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });


// Configurar Entity Framework con SQL Server con pool y reintentos
var configuredCs = builder.Configuration.GetConnectionString("DefaultConnection");
var envCs = Environment.GetEnvironmentVariable("SAUNA_CONNECTION_STRING");
var finalCs = string.IsNullOrWhiteSpace(envCs) ? configuredCs : envCs;
builder.Services.AddDbContextPool<SaunaDbContext>(options =>
{
    options.UseSqlServer(finalCs, sql =>
    {
        sql.EnableRetryOnFailure(5);
        sql.CommandTimeout(30);
    });
});

// Registrar repositorios
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
builder.Services.AddScoped<ICategoriaProductoRepository, CategoriaProductoRepository>();
builder.Services.AddScoped<ITipoMovimientoRepository, TipoMovimientoRepository>();
builder.Services.AddScoped<ITipoComprobanteRepository, TipoComprobanteRepository>();
builder.Services.AddScoped<IMetodoPagoRepository, MetodoPagoRepository>();
builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<ICategoriaServicioRepository, CategoriaServicioRepository>();
builder.Services.AddScoped<IDetalleServicioRepository, DetalleServicioRepository>();

builder.Services.AddScoped<IDetalleConsumoRepository, DetalleConsumoRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();


builder.Services.AddScoped<IEgresoRepository, EgresoRepository>();
builder.Services.AddScoped<ITipoEgresoRepository, TipoEgresoRepository>();


builder.Services.AddScoped<IPromocionRepository, PromocionRepository>();
builder.Services.AddScoped<ITipoDescuentoRepository, TipoDescuentoRepository>();
builder.Services.AddScoped<ICuentaRepository, CuentaRepository>();


// Registrar servicios
builder.Services.AddScoped<IAuthService, AuthenticationService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<EgresoService>();
builder.Services.AddScoped<FileUploadService>();

builder.Services.AddScoped<IPromocionService, PromocionService>();
builder.Services.AddScoped<ICuentaService, CuentaService>();
builder.Services.AddScoped<IDetalleServicioService, DetalleServicioService>();
builder.Services.AddScoped<IDetalleConsumoService, DetalleConsumoService>();
builder.Services.AddScoped<IMetodoPagoService, MetodoPagoService>();
builder.Services.AddScoped<ITipoComprobanteService, TipoComprobanteService>();
builder.Services.AddScoped<IComprobanteService, ComprobanteService>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddHttpContextAccessor();


// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false; // Desactivar sliding expiration
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;
    });

// Configurar autorización con roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("Usuario", "Administrador"));
    options.AddPolicy("RequireClienteAccess", policy => policy.RequireRole("Usuario", "Administrador", "Cajero"));
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Configurar sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();

app.MapHub<InventarioHub>("/hubs/inventario");

app.MapControllers();


// Redirigir la raíz al login
app.MapGet("/", context => 
{
    if (!context.User.Identity?.IsAuthenticated == true)
    {
        context.Response.Redirect("/Auth/Login");
        return Task.CompletedTask;
    }
    context.Response.Redirect("/Dashboard");
    return Task.CompletedTask;
});

app.Run();
