using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Mappers
{
    public static class TipoComprobanteMapper
    {
        public static TipoComprobanteDto ToDto(TipoComprobante t)
        {
            return new TipoComprobanteDto
            {
                IdTipoComprobante = t.IdTipoComprobante,
                Nombre = t.Nombre
            };
        }
    }
}
