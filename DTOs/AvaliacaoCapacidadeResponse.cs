namespace Fluxo_GS_2026_2TDSPR.DTOs;

public record AvaliacaoCapacidadeResponse(int PlataformaId, int TremId, DateTime AvaliadoEm,
    int QuantidadePessoas, long CapacidadeDisponivel, long PessoasSemEspaco,
    bool AvaliarReforco, string Mensagem);
