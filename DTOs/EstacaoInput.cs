using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.DTOs;

public record EstacaoInput([Required, StringLength(2000)] string Nome, [StringLength(300)] string? Endereco = null, [StringLength(100)] string? Cidade = null);
