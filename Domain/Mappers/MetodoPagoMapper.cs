using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Mappers
{
    public static class MetodoPagoMapper
    {
        public static MetodoPagoDto ToDto(MetodoPago m)
        {
            return new MetodoPagoDto
            {
                IdMetodoPago = m.IdMetodoPago,
                Nombre = m.Nombre
            };
        }
    }
}
