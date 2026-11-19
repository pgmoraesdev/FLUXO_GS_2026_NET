using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.Models
{
    public class Estacao
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Endereco { get; set; }

        [StringLength(100)]
        public string? Cidade { get; set; }

        public ICollection<Plataforma> Plataformas { get; set; } = new List<Plataforma>();
    }
}