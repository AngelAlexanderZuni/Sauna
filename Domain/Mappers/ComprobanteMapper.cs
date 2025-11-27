using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Mappers
{
    public static class ComprobanteMapper
    {
        public static ComprobanteDto ToDto(Comprobante c)
        {
            return new ComprobanteDto
            {
                IdComprobante = c.IdComprobante,
                Serie = c.Serie,
                Numero = c.Numero,
                FechaEmision = c.FechaEmision,
                Subtotal = c.Subtotal,
                Igv = c.Igv,
                Total = c.Total,
                IdTipoComprobante = c.IdTipoComprobante,
                TipoNombre = c.TipoComprobante?.Nombre ?? string.Empty,
                IdCuenta = c.IdCuenta
            };
        }
    }
}
