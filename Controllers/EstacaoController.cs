using System.ComponentModel.DataAnnotations;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fluxo_GS_2026_2TDSPR.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EstacaoController : ControllerBase
{
    private readonly AppDbContext dbContext;

    public EstacaoController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public record EstacaoInput([Required, StringLength(2000)] string Nome, [StringLength(300)] string? Endereco = null, [StringLength(100)] string? Cidade = null);

    public record EstacaoResponse(int Id, string Nome, string? Endereco, string? Cidade);

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EstacaoResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var estacoes = await dbContext.Estacoes
            .AsNoTracking()
            .OrderBy(estacao => estacao.Id)
            .Select(estacao => new EstacaoResponse(estacao.Id, estacao.Nome, estacao.Endereco, estacao.Cidade))
            .ToListAsync(cancellationToken);

        return Ok(estacoes);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstacaoResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var estacao = await dbContext.Estacoes
            .AsNoTracking()
            .Where(estacao => estacao.Id == id)
            .Select(estacao => new EstacaoResponse(estacao.Id, estacao.Nome, estacao.Endereco, estacao.Cidade))
            .SingleOrDefaultAsync(cancellationToken);

        if (estacao is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Estação não encontrada.");
        }

        return Ok(estacao);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EstacaoResponse>> Create(EstacaoInput input, CancellationToken cancellationToken)
    {
        Estacao estacao;
        try
        {
            estacao = new Estacao(input.Nome, input.Endereco, input.Cidade);
        }
        catch (ValidationException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: exception.Message);
        }
        dbContext.Estacoes.Add(estacao);

        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = estacao.Id }, new EstacaoResponse(estacao.Id, estacao.Nome, estacao.Endereco, estacao.Cidade));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, EstacaoInput input, CancellationToken cancellationToken)
    {
        var estacao = await dbContext.Estacoes.FindAsync(new object[] { id }, cancellationToken);
        if (estacao is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Estação não encontrada.");
        }

        try
        {
            estacao.AtualizarCadastro(input.Nome, input.Endereco, input.Cidade);
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
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Estação não encontrada.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var estacao = await dbContext.Estacoes.FindAsync(new object[] { id }, cancellationToken);
        if (estacao is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Estação não encontrada.");
        }

        dbContext.Estacoes.Remove(estacao);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Estação não encontrada.");
        }

        return NoContent();
    }
}
