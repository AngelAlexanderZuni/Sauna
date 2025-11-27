using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Mappers
{
    public static class DetalleServicioMapper
    {
        public static DetalleServicioDto ToDto(DetalleServicio d)
        {
            return new DetalleServicioDto
            {
                IdDetalleServicio = d.IdDetalleServicio,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal,
                IdCuenta = d.IdCuenta,
                IdServicio = d.IdServicio,
                ServicioNombre = d.Servicio?.Nombre
            };
        }
    }
}

