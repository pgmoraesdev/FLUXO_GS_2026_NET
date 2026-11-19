namespace Fluxo_GS_2026_2TDSPR.DTOs;

public record VagaoResponse(int Id, int TremId, int Posicao, int CapacidadePessoas,
    int? QuantidadePessoas, DateTime? AtualizadoEm);
