using System.ComponentModel.DataAnnotations;

namespace Fluxo_GS_2026_2TDSPR.Models;

public class Trem
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Codigo { get; private set; } = string.Empty;

    [StringLength(100)]
    public string? Modelo { get; private set; }

    [StringLength(100)]
    public string? Fabricante { get; private set; }

    private Trem() { }

    public Trem(string codigo, string? modelo = null, string? fabricante = null)
    {
        AtualizarCadastro(codigo, modelo, fabricante);
    }

    public void AtualizarCadastro(string codigo, string? modelo = null, string? fabricante = null)
    {
        Validator.ValidateProperty(codigo, new ValidationContext(this) { MemberName = nameof(Codigo) });
        Validator.ValidateProperty(modelo, new ValidationContext(this) { MemberName = nameof(Modelo) });
        Validator.ValidateProperty(fabricante, new ValidationContext(this) { MemberName = nameof(Fabricante) });

        Codigo = codigo.Trim();
        Modelo = string.IsNullOrWhiteSpace(modelo) ? null : modelo.Trim();
        Fabricante = string.IsNullOrWhiteSpace(fabricante) ? null : fabricante.Trim();
    }

    public ICollection<Vagao> Vagoes { get; set; } = new List<Vagao>();
}
