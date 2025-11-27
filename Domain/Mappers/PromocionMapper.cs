using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Mappers
{
    public static class PromocionMapper
    {
        public static PromocionDto ToDto(Promocion p)
        {
            return new PromocionDto
            {
                IdPromocion = p.IdPromocion,
                NombreDescuento = p.NombreDescuento,
                MontoDescuento = p.MontoDescuento,
                ValorCondicion = p.ValorCondicion,
                Activo = p.Activo,
                Motivo = p.Motivo,
                IdTipoDescuento = p.IdTipoDescuento,
                TipoNombre = p.TipoDescuento?.Nombre ?? string.Empty
            };
        }
    }
}

