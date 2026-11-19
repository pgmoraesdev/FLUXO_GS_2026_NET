using Fluxo_GS_2026_2TDSPR.Models;

namespace Fluxo_GS_2026_2TDSPR.DTOs;

// Resposta de consulta, sem tabela no banco. As faixas são da simulação acadêmica.
public record LotacaoResponse(int VagaoId, int Posicao, int CapacidadePessoas,
    int? OcupacaoEstimada, decimal? PercentualOcupacao, DateTime? MedidaEm,
    string Situacao, string Cor)
{
    public static LotacaoResponse Criar(Vagao vagao, DateTime agora)
    {
        if (vagao.QuantidadePessoas is null or < 0 || vagao.CapacidadePessoas <= 0
            || vagao.AtualizadoEm is null || vagao.AtualizadoEm > agora
            || vagao.AtualizadoEm < agora.AddMinutes(-5))
        {
            return new(vagao.Id, vagao.Posicao, vagao.CapacidadePessoas,
                null, null, vagao.AtualizadoEm, "Lotação indisponível", "Cinza");
        }

        decimal percentual = 100m * vagao.QuantidadePessoas.Value / vagao.CapacidadePessoas;
        string situacao;
        string cor;

        if (percentual < 50)
        {
            situacao = "Com espaço";
            cor = "Verde";
        }
        else if (percentual < 80)
        {
            situacao = "Ocupado";
            cor = "Amarelo";
        }
        else
        {
            situacao = "Muito cheio";
            cor = "Vermelho";
        }

        return new(vagao.Id, vagao.Posicao, vagao.CapacidadePessoas,
            vagao.QuantidadePessoas, percentual, vagao.AtualizadoEm, situacao, cor);
    }
}
