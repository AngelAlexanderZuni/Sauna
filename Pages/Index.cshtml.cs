using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using ProyectoSaunaKalixto.Web.Domain.Services;

namespace ProyectoSaunaKalixto.Web.Pages;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IClienteService _clienteService;

    public IndexModel(ILogger<IndexModel> logger, IClienteService clienteService)
    {
        _logger = logger;
        _clienteService = clienteService;
    }

    public int TotalClientes { get; set; }
    public int ClientesActivos { get; set; }
    public int MembresiasPorVencer { get; set; }
    public int RegistrosHoy { get; set; }

    public void OnGet()
    {
        // La redirección se maneja en Program.cs
        // Esta página se usa solo como punto de entrada
    }
}