using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.Models;

public class Vagao
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int TremId { get; set; }

    [Range(1, int.MaxValue)]
    public int Posicao { get; set; }

    [Range(1, int.MaxValue)]
    public int CapacidadePessoas { get; set; }

    [Range(0, int.MaxValue)]
    public int? QuantidadePessoas { get; set; }

    public DateTime? AtualizadoEm { get; set; }

    public Trem? Trem { get; set; }
}
