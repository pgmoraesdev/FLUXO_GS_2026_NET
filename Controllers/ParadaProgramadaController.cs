using System.ComponentModel.DataAnnotations;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace Fluxo_GS_2026_2TDSPR.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParadaProgramadaController : ControllerBase
{
    private readonly AppDbContext dbContext;

    public ParadaProgramadaController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public record ParadaInput(
        [Range(1, int.MaxValue)] int TremId,
        [Range(1, int.MaxValue)] int PlataformaId,
        DateTime ChegadaPrevistaEm,
        DateTime? ChegadaRealEm) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ChegadaPrevistaEm == default || ChegadaPrevistaEm.Kind != DateTimeKind.Utc)
                yield return new ValidationResult("Informe a chegada prevista em UTC, com sufixo Z.", new[] { nameof(ChegadaPrevistaEm) });
            if (ChegadaRealEm is { } real && (real == default || real.Kind != DateTimeKind.Utc))
                yield return new ValidationResult("Informe a chegada real em UTC, com sufixo Z.", new[] { nameof(ChegadaRealEm) });
        }
    }

    public record ParadaResponse(int Id, int TremId, int PlataformaId,
        DateTime ChegadaPrevistaEm, DateTime? ChegadaRealEm);

    // Oracle TIMESTAMP não guarda fuso; todos os horários persistidos são UTC.
    private static ParadaResponse ToResponse(ParadaProgramada parada) => new(
        parada.Id, parada.TremId, parada.PlataformaId,
        DateTime.SpecifyKind(parada.ChegadaPrevistaEm, DateTimeKind.Utc),
        parada.ChegadaRealEm is { } real ? DateTime.SpecifyKind(real, DateTimeKind.Utc) : null);

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ParadaResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var paradas = await dbContext.ParadasProgramadas.AsNoTracking()
            .OrderBy(p => p.ChegadaPrevistaEm).ThenBy(p => p.Id).ToListAsync(cancellationToken);
        return Ok(paradas.Select(ToResponse).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParadaResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var parada = await dbContext.ParadasProgramadas.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        return parada is null
            ? Problem(statusCode: 404, detail: "Parada programada não encontrada.")
            : Ok(ToResponse(parada));
    }

    [HttpGet("/api/Plataforma/{id}/proxima-chegada")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParadaResponse>> GetProximaChegada(int id, CancellationToken cancellationToken)
    {
        if (!await dbContext.Set<Plataforma>().AnyAsync(p => p.Id == id, cancellationToken))
            return Problem(statusCode: 404, detail: "Plataforma não encontrada.");

        var agora = DateTime.UtcNow;
        var parada = await dbContext.ParadasProgramadas.AsNoTracking()
            .Where(p => p.PlataformaId == id && p.ChegadaRealEm == null && p.ChegadaPrevistaEm >= agora)
            .OrderBy(p => p.ChegadaPrevistaEm).ThenBy(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);
        return parada is null
            ? Problem(statusCode: 404, detail: "Nenhuma próxima chegada programada para esta plataforma.")
            : Ok(ToResponse(parada));
    }

    private async Task<ActionResult?> ValidarVinculos(ParadaInput input, CancellationToken cancellationToken)
    {
        if (!await dbContext.Trens.AnyAsync(t => t.Id == input.TremId, cancellationToken))
            return Problem(statusCode: 404, detail: "Trem não encontrado.");
        if (!await dbContext.Set<Plataforma>().AnyAsync(p => p.Id == input.PlataformaId, cancellationToken))
            return Problem(statusCode: 404, detail: "Plataforma não encontrada.");
        return null;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ParadaResponse>> Create(ParadaInput input, CancellationToken cancellationToken)
    {
        var erro = await ValidarVinculos(input, cancellationToken);
        if (erro is not null) return erro;
        var parada = new ParadaProgramada
        {
            TremId = input.TremId, PlataformaId = input.PlataformaId,
            ChegadaPrevistaEm = input.ChegadaPrevistaEm, ChegadaRealEm = input.ChegadaRealEm
        };
        dbContext.ParadasProgramadas.Add(parada);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 2291 })
        {
            return Problem(statusCode: 404, detail: "Trem ou plataforma não encontrado.");
        }
        return CreatedAtAction(nameof(GetById), new { id = parada.Id }, ToResponse(parada));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, ParadaInput input, CancellationToken cancellationToken)
    {
        var parada = await dbContext.ParadasProgramadas.FindAsync(new object[] { id }, cancellationToken);
        if (parada is null) return Problem(statusCode: 404, detail: "Parada programada não encontrada.");
        var erro = await ValidarVinculos(input, cancellationToken);
        if (erro is not null) return erro;
        if (parada.TremId != input.TremId && await dbContext.MedicoesVagao
            .AnyAsync(m => m.ParadaProgramadaId == id, cancellationToken))
        {
            return Problem(statusCode: 409, detail: "A parada tem medições de vagões. Remova esses vínculos antes de trocar o trem.");
        }

        if ((parada.TremId != input.TremId || parada.PlataformaId != input.PlataformaId)
            && await dbContext.AlertasOperacionais.AnyAsync(a => a.ParadaProgramadaId == id, cancellationToken))
        {
            return Problem(statusCode: 409, detail: "A parada tem um alerta. Remova o alerta antes de trocar o trem ou a plataforma.");
        }

        parada.TremId = input.TremId;
        parada.PlataformaId = input.PlataformaId;
        parada.ChegadaPrevistaEm = input.ChegadaPrevistaEm;
        parada.ChegadaRealEm = input.ChegadaRealEm;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: 404, detail: "Parada programada não encontrada.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 2291 })
        {
            return Problem(statusCode: 404, detail: "Trem ou plataforma não encontrado.");
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var parada = await dbContext.ParadasProgramadas.FindAsync(new object[] { id }, cancellationToken);
        if (parada is null) return Problem(statusCode: 404, detail: "Parada programada não encontrada.");
        dbContext.ParadasProgramadas.Remove(parada);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 2292 })
        {
            return Problem(statusCode: 409, detail: "Existem medições ou alertas vinculados. Remova esses registros antes de excluir este recurso.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: 404, detail: "Parada programada não encontrada.");
        }
        return NoContent();
    }
}
