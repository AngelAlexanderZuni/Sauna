using ProyectoSaunaKalixto.Web.Domain.DTOs;
using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.Mappers
{
    public static class PagoMapper
    {
        public static PagoDto ToDto(Pago p)
        {
            return new PagoDto
            {
                IdPago = p.IdPago,
                FechaHora = p.FechaHora,
                Monto = p.Monto,
                NumeroReferencia = p.NumeroReferencia,
                IdMetodoPago = p.IdMetodoPago,
                MetodoNombre = p.MetodoPago?.Nombre ?? string.Empty,
                IdCuenta = p.IdCuenta
            };
        }
    }
}
