namespace Fluxo_GS_2026_2TDSPR.Models;

public class ParadaProgramada
{
    public int Id { get; set; }
    public int TremId { get; set; }
    public int PlataformaId { get; set; }
    public DateTime ChegadaPrevistaEm { get; set; }
    public DateTime? ChegadaRealEm { get; set; }
    public Trem? Trem { get; set; }
    public Plataforma? Plataforma { get; set; }
}
