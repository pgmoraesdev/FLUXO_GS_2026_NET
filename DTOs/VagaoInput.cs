using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.DTOs;

public record VagaoInput(
    [Range(1, int.MaxValue)] int TremId,
    [Range(1, int.MaxValue)] int Posicao,
    [Range(1, int.MaxValue)] int CapacidadePessoas,
    [Range(0, int.MaxValue)] int? QuantidadePessoas = null);
