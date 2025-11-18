using Microsoft.EntityFrameworkCore;
using ProyectoSaunaKalixto.Web.Data;
using ProyectoSaunaKalixto.Web.Domain.Models;
using System.Linq;

namespace ProyectoSaunaKalixto.Web.Domain.Repositories
{
    public class ClienteRepository : Repository<Cliente>, IClienteRepository
    {
        public ClienteRepository(SaunaDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Cliente>> GetByNumeroDocumentoAsync(string numeroDocumento)
        {
            return await _context.Clientes
                .Where(c => c.NumeroDocumento.Contains(numeroDocumento) && c.Activo)
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNumeroDocumentoAsync(string numeroDocumento)
        {
            return await _context.Clientes
                .AnyAsync(c => c.NumeroDocumento == numeroDocumento);
        }

        public async Task<IEnumerable<Cliente>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllAsync();
            }

            searchTerm = searchTerm.ToLower();
            
            return await _context.Clientes
                .Where(c => (c.NumeroDocumento.Contains(searchTerm) || 
                           c.Nombre.ToLower().Contains(searchTerm) || 
                           c.Apellido.ToLower().Contains(searchTerm)) && 
                           c.Activo)
                .OrderBy(c => c.Apellido)
                .ThenBy(c => c.Nombre)
                .ToListAsync();
        }
    }
}