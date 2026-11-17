using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.Models;

public class Trem
{
    public int Id { get; set; }

    [Required]
    public string Codigo { get; set; } = string.Empty;

    public ICollection<Vagao> Vagoes { get; set; } = new List<Vagao>();
}
