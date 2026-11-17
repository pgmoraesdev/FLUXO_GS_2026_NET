namespace Fluxo_GS_2026_2TDSPR.Models;

// Guarda quantas pessoas estavam na área de embarque em um determinado horário.
public class MedicaoPlataforma
{
    public int Id { get; set; }
    public int PlataformaId { get; set; }
    public int QuantidadePessoas { get; set; }
    public DateTime MedidaEm { get; set; }
    public string Origem { get; set; } = string.Empty;
}
