using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Usuarios
{
    [Authorize(Policy = "RequireUserRole")]
    public class IndexModel : PageModel
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IUsuarioService usuarioService, ILogger<IndexModel> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        public IEnumerable<UsuarioDTO> Usuarios { get; set; } = new List<UsuarioDTO>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowInactive { get; set; } = false;

        public async Task OnGetAsync()
        {
            try
            {
                // Obtener todos los usuarios
                var todosUsuarios = (await _usuarioService.GetAllUsuariosAsync()).ToList();

                // Filtrar según estado activo/inactivo
                Usuarios = ShowInactive 
                    ? todosUsuarios.Where(u => !u.Activo).ToList()
                    : todosUsuarios.Where(u => u.Activo).ToList();

                _logger.LogInformation($"Usuarios cargados: {Usuarios.Count()} ({(ShowInactive ? "inactivos" : "activos")})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de usuarios");
                Usuarios = new List<UsuarioDTO>();
            }
        }
    }
}