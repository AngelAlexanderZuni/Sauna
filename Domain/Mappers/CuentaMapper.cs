using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Mappers
{
    public static class CuentaMapper
    {
        public static CuentaDto ToDto(Cuenta c)
        {
            return new CuentaDto
            {
                IdCuenta = c.IdCuenta,
                FechaHoraCreacion = c.FechaHoraCreacion,
                FechaHoraSalida = c.FechaHoraSalida,
                SubtotalServicios = 0,
                SubtotalProductos = 0,
                SubtotalConsumos = c.SubtotalConsumos,
                Descuento = c.Descuento,
                Total = c.Total,
                IdEstadoCuenta = c.IdEstadoCuenta,
                EstadoNombre = c.EstadoCuenta?.Nombre ?? string.Empty,
                IdUsuarioCreador = c.IdUsuarioCreador,
                IdCliente = c.IdCliente,
                ClienteNombre = c.Cliente != null ? (c.Cliente.Nombre + " " + c.Cliente.Apellido) : string.Empty,
                IdPromocion = c.IdPromocion,
                PromocionNombre = c.Promocion?.NombreDescuento,
                PromocionMontoDescuento = c.Promocion?.MontoDescuento
            };
        }
    }
}

