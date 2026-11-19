using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.Models
{
    public class Estacao
    {
        public int Id { get; set; }

        [Required]
        public string Nome { get; private set; } = string.Empty;

        [StringLength(300)]
        public string? Endereco { get; private set; }

        [StringLength(100)]
        public string? Cidade { get; private set; }

        private Estacao() { }

        public Estacao(string nome, string? endereco = null, string? cidade = null)
        {
            AtualizarCadastro(nome, endereco, cidade);
        }

        public void AtualizarCadastro(string nome, string? endereco = null, string? cidade = null)
        {
            Validator.ValidateProperty(nome, new ValidationContext(this) { MemberName = nameof(Nome) });
            if (nome.Length > 2000)
                throw new ValidationException("O nome da estação deve ter no máximo 2000 caracteres.");
            Validator.ValidateProperty(endereco, new ValidationContext(this) { MemberName = nameof(Endereco) });
            Validator.ValidateProperty(cidade, new ValidationContext(this) { MemberName = nameof(Cidade) });

            Nome = nome.Trim();
            Endereco = string.IsNullOrWhiteSpace(endereco) ? null : endereco.Trim();
            Cidade = string.IsNullOrWhiteSpace(cidade) ? null : cidade.Trim();
        }

        public ICollection<Plataforma> Plataformas { get; set; } = new List<Plataforma>();
    }
}
