using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages.Clientes
{
    [Authorize(Policy = "RequireClienteAccess")]
    public class IndexModel : PageModel
    {
        private readonly IClienteService _clienteService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IClienteService clienteService, ILogger<IndexModel> logger)
        {
            _clienteService = clienteService;
            _logger = logger;
        }

        public IEnumerable<ClienteDTO> Clientes { get; set; } = new List<ClienteDTO>();
        
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public bool ShowInactive { get; set; } = false;

        public async Task OnGetAsync()
        {
            try
            {
                IEnumerable<ClienteDTO> todosClientes;
                
                if (string.IsNullOrWhiteSpace(SearchTerm))
                {
                    todosClientes = await _clienteService.GetAllClientesAsync();
                }
                else
                {
                    // Si el término de búsqueda parece un número de documento (solo números), buscar por número de documento
                    if (SearchTerm.All(char.IsDigit))
                    {
                        todosClientes = await _clienteService.GetClientesByNumeroDocumentoAsync(SearchTerm);
                    }
                    else
                    {
                        todosClientes = await _clienteService.SearchClientesAsync(SearchTerm);
                    }
                }
                
                // Filtrar por estado (activo/inactivo)
                Clientes = ShowInactive 
                    ? todosClientes.Where(c => !c.Activo).ToList()
                    : todosClientes.Where(c => c.Activo).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de clientes");
                Clientes = new List<ClienteDTO>();
            }
        }
    }
}