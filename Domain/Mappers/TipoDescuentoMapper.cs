using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Mappers
{
    public static class TipoDescuentoMapper
    {
        public static TipoDescuentoDto ToDto(TipoDescuento t)
        {
            return new TipoDescuentoDto
            {
                IdTipoDescuento = t.IdTipoDescuento,
                Nombre = t.Nombre
            };
        }
    }
}

