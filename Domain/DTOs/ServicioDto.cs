using ProyectoSaunaKalixto.Web.Domain.Models;

namespace ProyectoSaunaKalixto.Web.Domain.DTOs
{
    public class ServicioDto
    {
        public int IdServicio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int DuracionMinutos { get; set; }
        public int? IdCategoriaServicio { get; set; }
        public string? CategoriaNombre { get; set; }
        public bool Activo { get; set; }

        public static ServicioDto FromEntity(Servicio s)
        {
            return new ServicioDto
            {
                IdServicio = s.IdServicio,
                Nombre = s.Nombre,
                Descripcion = null,
                Precio = s.Precio,
                DuracionMinutos = s.DuracionEstimada.GetValueOrDefault(),
                IdCategoriaServicio = s.IdCategoriaServicio,
                CategoriaNombre = s.CategoriaServicio?.Nombre,
                Activo = s.Activo
            };
        }
    }
}

