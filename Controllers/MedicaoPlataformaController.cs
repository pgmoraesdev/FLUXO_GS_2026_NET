using System.ComponentModel.DataAnnotations;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace Fluxo_GS_2026_2TDSPR.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicaoPlataformaController : ControllerBase
{
    private readonly AppDbContext dbContext;

    public MedicaoPlataformaController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    // Campos anuláveis permitem distinguir um valor ausente de uma contagem válida de zero.
    public record MedicaoInput(
        [Range(1, int.MaxValue)] int PlataformaId,
        [Required, Range(0, int.MaxValue)] int? QuantidadePessoas,
        [Required] DateTime? MedidaEm,
        [Required, StringLength(50)] string Origem) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MedidaEm is { } data && (data == default || data.Kind != DateTimeKind.Utc || data > DateTime.UtcNow))
            {
                yield return new ValidationResult(
                    "Informe um horário UTC com sufixo Z, que não esteja no futuro.",
                    new[] { nameof(MedidaEm) });
            }
        }
    }

    // Oracle TIMESTAMP não guarda fuso. Todos os horários desta API são UTC.
    private static MedicaoPlataforma ComHorarioUtc(MedicaoPlataforma medicao)
    {
        medicao.MedidaEm = DateTime.SpecifyKind(medicao.MedidaEm, DateTimeKind.Utc);
        return medicao;
    }

    [HttpGet]
    public async Task<ActionResult<List<MedicaoPlataforma>>> GetAll(CancellationToken cancellationToken)
    {
        var medicoes = await dbContext.MedicoesPlataforma.AsNoTracking()
            .OrderByDescending(m => m.MedidaEm).ThenByDescending(m => m.Id)
            .ToListAsync(cancellationToken);

        return Ok(medicoes.Select(ComHorarioUtc).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicaoPlataforma>> GetById(int id, CancellationToken cancellationToken)
    {
        var medicao = await dbContext.MedicoesPlataforma.AsNoTracking()
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (medicao is null)
            return Problem(statusCode: 404, detail: "Medição não encontrada.");

        return Ok(ComHorarioUtc(medicao));
    }

    [HttpGet("/api/Plataforma/{id}/ultima-ocupacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicaoPlataforma>> GetUltimaOcupacao(int id, CancellationToken cancellationToken)
    {
        if (!await dbContext.Set<Plataforma>().AnyAsync(p => p.Id == id, cancellationToken))
            return Problem(statusCode: 404, detail: "Plataforma não encontrada.");

        var agora = DateTime.UtcNow;
        var medicao = await dbContext.MedicoesPlataforma.AsNoTracking()
            .Where(m => m.PlataformaId == id && m.MedidaEm <= agora)
            .OrderByDescending(m => m.MedidaEm).ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (medicao is null)
            return Problem(statusCode: 404, detail: "Nenhuma medição de ocupação encontrada para esta plataforma.");

        return Ok(ComHorarioUtc(medicao));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicaoPlataforma>> Create(MedicaoInput input, CancellationToken cancellationToken)
    {
        if (!await dbContext.Set<Plataforma>().AnyAsync(p => p.Id == input.PlataformaId, cancellationToken))
            return Problem(statusCode: 404, detail: "Plataforma não encontrada.");

        var medicao = new MedicaoPlataforma
        {
            PlataformaId = input.PlataformaId,
            QuantidadePessoas = input.QuantidadePessoas!.Value,
            MedidaEm = input.MedidaEm!.Value,
            Origem = input.Origem.Trim()
        };
        dbContext.MedicoesPlataforma.Add(medicao);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 2291 })
        {
            return Problem(statusCode: 404, detail: "Plataforma não encontrada.");
        }

        return CreatedAtAction(nameof(GetById), new { id = medicao.Id }, medicao);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, MedicaoInput input, CancellationToken cancellationToken)
    {
        var medicao = await dbContext.MedicoesPlataforma.FindAsync(new object[] { id }, cancellationToken);
        if (medicao is null)
            return Problem(statusCode: 404, detail: "Medição não encontrada.");

        if (!await dbContext.Set<Plataforma>().AnyAsync(p => p.Id == input.PlataformaId, cancellationToken))
            return Problem(statusCode: 404, detail: "Plataforma não encontrada.");

        medicao.PlataformaId = input.PlataformaId;
        medicao.QuantidadePessoas = input.QuantidadePessoas!.Value;
        medicao.MedidaEm = input.MedidaEm!.Value;
        medicao.Origem = input.Origem.Trim();

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: 404, detail: "Medição não encontrada.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 2291 })
        {
            return Problem(statusCode: 404, detail: "Plataforma não encontrada.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var medicao = await dbContext.MedicoesPlataforma.FindAsync(new object[] { id }, cancellationToken);
        if (medicao is null)
            return Problem(statusCode: 404, detail: "Medição não encontrada.");

        dbContext.MedicoesPlataforma.Remove(medicao);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: 404, detail: "Medição não encontrada.");
        }

        return NoContent();
    }
}
