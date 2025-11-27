using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Mappers
{
    public static class DetalleConsumoMapper
    {
        public static DetalleConsumoDto ToDto(DetalleConsumo d)
        {
            return new DetalleConsumoDto
            {
                IdDetalle = d.IdDetalle,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal,
                IdCuenta = d.IdCuenta,
                IdProducto = d.IdProducto,
                ProductoNombre = d.Producto?.Nombre,
                ProductoCodigo = d.Producto?.Codigo
            };
        }
    }
}

