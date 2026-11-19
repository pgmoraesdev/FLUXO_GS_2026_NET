using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.DTOs;

public record PlataformaInput(
    [Required, StringLength(2000)] string Nome,
    [Required, StringLength(2000)] string Sentido,
    [Range(1, int.MaxValue)] int EstacaoId,
    [Range(0, int.MaxValue)] int? QuantidadePessoas = null);
