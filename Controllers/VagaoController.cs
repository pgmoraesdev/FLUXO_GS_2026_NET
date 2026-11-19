using System.ComponentModel.DataAnnotations;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Fluxo_GS_2026_2TDSPR.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace Fluxo_GS_2026_2TDSPR.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VagaoController : ControllerBase
{
    private readonly AppDbContext dbContext;

    public VagaoController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public record VagaoInput(
        [Range(1, int.MaxValue)] int TremId,
        [Range(1, int.MaxValue)] int Posicao,
        [Range(1, int.MaxValue)] int CapacidadePessoas,
        [Range(0, int.MaxValue)] int? QuantidadePessoas = null);

    public record VagaoResponse(int Id, int TremId, int Posicao, int CapacidadePessoas,
        int? QuantidadePessoas, DateTime? AtualizadoEm);

    [HttpGet("{id}/lotacao")]
    public async Task<ActionResult<LotacaoResponse>> GetLotacao(int id, CancellationToken cancellationToken)
    {
        var vagao = await dbContext.Vagoes.AsNoTracking()
            .SingleOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (vagao is null)
            return Problem(statusCode: 404, detail: "Vagão não encontrado.");

        return Ok(LotacaoResponse.Criar(vagao, DateTime.UtcNow));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VagaoResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var vagoes = await dbContext.Vagoes
            .AsNoTracking()
            .OrderBy(vagao => vagao.Id)
            .Select(vagao => new VagaoResponse(vagao.Id, vagao.TremId, vagao.Posicao, vagao.CapacidadePessoas,
                vagao.QuantidadePessoas, vagao.AtualizadoEm))
            .ToListAsync(cancellationToken);

        return Ok(vagoes);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VagaoResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var vagao = await dbContext.Vagoes
            .AsNoTracking()
            .Where(vagao => vagao.Id == id)
            .Select(vagao => new VagaoResponse(vagao.Id, vagao.TremId, vagao.Posicao, vagao.CapacidadePessoas,
                vagao.QuantidadePessoas, vagao.AtualizadoEm))
            .SingleOrDefaultAsync(cancellationToken);

        if (vagao is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Vagão não encontrado.");
        }

        return Ok(vagao);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VagaoResponse>> Create(VagaoInput input, CancellationToken cancellationToken)
    {
        if (!await dbContext.Trens.AnyAsync(trem => trem.Id == input.TremId, cancellationToken))
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Trem não encontrado.");
        }

        Vagao vagao;
        try
        {
            vagao = new Vagao(input.TremId, input.Posicao, input.CapacidadePessoas, input.QuantidadePessoas);
        }
        catch (ValidationException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: exception.Message);
        }
        dbContext.Vagoes.Add(vagao);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is OracleException { Number: 2291 })
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Trem não encontrado.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is OracleException { Number: 1 })
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Já existe um vagão nessa posição para esse trem.");
        }

        return CreatedAtAction(nameof(GetById), new { id = vagao.Id }, new VagaoResponse(vagao.Id, vagao.TremId,
            vagao.Posicao, vagao.CapacidadePessoas, vagao.QuantidadePessoas, vagao.AtualizadoEm));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, VagaoInput input, CancellationToken cancellationToken)
    {
        var vagao = await dbContext.Vagoes.FindAsync(new object[] { id }, cancellationToken);
        if (vagao is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Vagão não encontrado.");
        }

        if (!await dbContext.Trens.AnyAsync(trem => trem.Id == input.TremId, cancellationToken))
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Trem não encontrado.");
        }

        try
        {
            vagao.AtualizarCadastro(input.TremId, input.Posicao, input.CapacidadePessoas, input.QuantidadePessoas);
        }
        catch (ValidationException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: exception.Message);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Vagão não encontrado.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is OracleException { Number: 2291 })
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Trem não encontrado.");
        }
        catch (DbUpdateException exception) when (exception.InnerException is OracleException { Number: 1 })
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "Já existe um vagão nessa posição para esse trem.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var vagao = await dbContext.Vagoes.FindAsync(new object[] { id }, cancellationToken);
        if (vagao is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Vagão não encontrado.");
        }

        dbContext.Vagoes.Remove(vagao);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Vagão não encontrado.");
        }

        return NoContent();
    }
}
