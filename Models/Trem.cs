using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.Models;

public class Trem
{
    public int Id { get; set; }

    [Required]
    public string Codigo { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Modelo { get; set; }

    [StringLength(100)]
    public string? Fabricante { get; set; }

    public ICollection<Vagao> Vagoes { get; set; } = new List<Vagao>();
}
