using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.Models;

public class Vagao
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int TremId { get; private set; }

    [Range(1, int.MaxValue)]
    public int Posicao { get; private set; }

    [Range(1, int.MaxValue)]
    public int CapacidadePessoas { get; private set; }

    [Range(0, int.MaxValue)]
    public int? QuantidadePessoas { get; private set; }

    public DateTime? AtualizadoEm { get; private set; }

    private Vagao() { }

    public Vagao(int tremId, int posicao, int capacidadePessoas, int? quantidadePessoas = null)
    {
        AtualizarCadastro(tremId, posicao, capacidadePessoas, quantidadePessoas);
    }

    public void AtualizarCadastro(int tremId, int posicao, int capacidadePessoas, int? quantidadePessoas = null)
    {
        Validator.ValidateProperty(tremId, new ValidationContext(this) { MemberName = nameof(TremId) });
        Validator.ValidateProperty(posicao, new ValidationContext(this) { MemberName = nameof(Posicao) });
        Validator.ValidateProperty(capacidadePessoas, new ValidationContext(this) { MemberName = nameof(CapacidadePessoas) });

        AtualizarOcupacao(quantidadePessoas);
        TremId = tremId;
        Posicao = posicao;
        CapacidadePessoas = capacidadePessoas;
    }

    public void AtualizarOcupacao(int? quantidadePessoas)
    {
        Validator.ValidateProperty(quantidadePessoas, new ValidationContext(this) { MemberName = nameof(QuantidadePessoas) });

        QuantidadePessoas = quantidadePessoas;
        AtualizadoEm = quantidadePessoas.HasValue ? DateTime.UtcNow : null;
    }

    public Trem? Trem { get; set; }
}
