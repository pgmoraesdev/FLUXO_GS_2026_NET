using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.DTOs;

public record TremInput([Required, StringLength(50)] string Codigo, [StringLength(100)] string? Modelo = null, [StringLength(100)] string? Fabricante = null);
