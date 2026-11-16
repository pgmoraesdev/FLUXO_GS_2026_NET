namespace Fluxo_GS_2026_2TDSPR.Models;

public class Plataforma
{
    public int Id { get; set; }

    public int EstacaoId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Sentido { get; set; } = string.Empty;

    public Estacao? Estacao { get; set; }
}