namespace Fluxo_GS_2026_2TDSPR.Models;

public class AlertaOperacional
{
    public int Id { get; set; }
    public int ParadaProgramadaId { get; set; }
    public DateTime GeradoEm { get; set; }
    public int DemandaProjetada { get; set; }
    public long CapacidadeDisponivelEstimada { get; set; }
    public string Status { get; set; } = "Aberto";
    public string? AcaoOperador { get; set; }
    public DateTime? AtualizadoEm { get; set; }
}
