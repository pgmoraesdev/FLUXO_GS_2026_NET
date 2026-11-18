using System.ComponentModel.DataAnnotations;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace Fluxo_GS_2026_2TDSPR.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TremController : ControllerBase
{
    private readonly AppDbContext dbContext;

    public TremController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public record TremInput([Required, StringLength(50)] string Codigo);

    public record TremResponse(int Id, string Codigo);

    [HttpGet("{id}/lotacao")]
    public async Task<ActionResult<List<LotacaoResponse>>> GetLotacao(int id, CancellationToken cancellationToken)
    {
        if (!await dbContext.Trens.AnyAsync(t => t.Id == id, cancellationToken))
            return Problem(statusCode: 404, detail: "Trem não encontrado.");

        var vagoes = await dbContext.Vagoes.AsNoTracking()
            .Where(v => v.TremId == id).OrderBy(v => v.Posicao)
            .ToListAsync(cancellationToken);
        var agora = DateTime.UtcNow;
        return Ok(vagoes.Select(v => LotacaoResponse.Criar(v, agora)).ToList());
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TremResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var trens = await dbContext.Trens
            .AsNoTracking()
            .OrderBy(trem => trem.Id)
            .Select(trem => new TremResponse(trem.Id, trem.Codigo))
            .ToListAsync(cancellationToken);

        return Ok(trens);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TremResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var trem = await dbContext.Trens
            .AsNoTracking()
            .Where(trem => trem.Id == id)
            .Select(trem => new TremResponse(trem.Id, trem.Codigo))
            .SingleOrDefaultAsync(cancellationToken);

        if (trem is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Trem não encontrado.");
        }

        return Ok(trem);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TremResponse>> Create(TremInput input, CancellationToken cancellationToken)
    {
        var trem = new Trem { Codigo = input.Codigo.Trim() };
        dbContext.Trens.Add(trem);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is OracleException { Number: 1 })
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Já existe um trem com esse código.");
        }

        return CreatedAtAction(nameof(GetById), new { id = trem.Id }, new TremResponse(trem.Id, trem.Codigo));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, TremInput input, CancellationToken cancellationToken)
    {
        var trem = await dbContext.Trens.FindAsync(new object[] { id }, cancellationToken);
        if (trem is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Trem não encontrado.");
        }

        trem.Codigo = input.Codigo.Trim();

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Trem não encontrado.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is OracleException { Number: 1 })
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Já existe um trem com esse código.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var trem = await dbContext.Trens.FindAsync(new object[] { id }, cancellationToken);
        if (trem is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Trem não encontrado.");
        }

        dbContext.Trens.Remove(trem);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 2292 })
        {
            return Problem(statusCode: 409, detail: "Existem paradas ou medições vinculadas. Remova esses registros antes de excluir este recurso.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Trem não encontrado.");
        }

        return NoContent();
    }
}
