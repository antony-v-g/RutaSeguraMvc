using System.ComponentModel.DataAnnotations;

namespace RutaSeguraMvc.Models;

public class Reporte
{
    public int Id { get; set; }

    [Required]
    public string TipoIncidente { get; set; } = string.Empty;

    [Required]
    public string Descripcion { get; set; } = string.Empty;

    public string Ubicacion { get; set; } = string.Empty;

    public DateTime Fecha { get; set; } = DateTime.Now;

    public string Estado { get; set; } = "Pendiente";
}