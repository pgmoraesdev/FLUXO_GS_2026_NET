namespace Fluxo_GS_2026_2TDSPR.Models;

public class MedicaoVagao
{
    public int Id { get; set; }
    public int VagaoId { get; set; }
    public int? ParadaProgramadaId { get; set; }
    public int OcupacaoInicial { get; set; }
    public int Embarques { get; set; }
    public int Desembarques { get; set; }
    public int OcupacaoEstimada { get; set; }
    public DateTime MedidaEm { get; set; }
    public string Origem { get; set; } = string.Empty;
}
