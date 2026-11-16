namespace Fluxo_GS_2026_2TDSPR.Models
{
    public class Estacao
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public ICollection<Plataforma> Plataformas { get; set; } = new List<Plataforma>();
    }
}