using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.Models;

public class Plataforma
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int EstacaoId { get; private set; }

    [Required]
    public string Nome { get; private set; } = string.Empty;

    [Required]
    public string Sentido { get; private set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int? QuantidadePessoas { get; private set; }

    public DateTime? AtualizadoEm { get; private set; }

    private Plataforma() { }

    public Plataforma(string nome, string sentido, int estacaoId, int? quantidadePessoas = null)
    {
        AtualizarCadastro(nome, sentido, estacaoId, quantidadePessoas);
    }

    public void AtualizarCadastro(string nome, string sentido, int estacaoId, int? quantidadePessoas = null)
    {
        Validator.ValidateProperty(nome, new ValidationContext(this) { MemberName = nameof(Nome) });
        Validator.ValidateProperty(sentido, new ValidationContext(this) { MemberName = nameof(Sentido) });
        Validator.ValidateProperty(estacaoId, new ValidationContext(this) { MemberName = nameof(EstacaoId) });
        if (nome.Length > 2000 || sentido.Length > 2000)
            throw new ValidationException("O nome e o sentido da plataforma devem ter no máximo 2000 caracteres.");

        AtualizarOcupacao(quantidadePessoas);
        Nome = nome.Trim();
        Sentido = sentido.Trim();
        EstacaoId = estacaoId;
    }

    public void AtualizarOcupacao(int? quantidadePessoas)
    {
        Validator.ValidateProperty(quantidadePessoas, new ValidationContext(this) { MemberName = nameof(QuantidadePessoas) });

        QuantidadePessoas = quantidadePessoas;
        AtualizadoEm = quantidadePessoas.HasValue ? DateTime.UtcNow : null;
    }

    public Estacao? Estacao { get; set; }
}
