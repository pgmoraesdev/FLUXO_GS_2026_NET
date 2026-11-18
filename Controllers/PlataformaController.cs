using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fluxo_GS_2026_2TDSPR.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlataformaController : ControllerBase
{
    private readonly AppDbContext dbContext;

    public PlataformaController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public record PlataformaInput(
        [Required, StringLength(2000)] string Nome,
        [Required, StringLength(2000)] string Sentido,
        [Range(1, int.MaxValue)] int EstacaoId,
        [Range(0, int.MaxValue)] int? QuantidadePessoas = null);

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Plataforma>>> GetAll(CancellationToken cancellationToken)
    {
        var plataformas = await dbContext.Set<Plataforma>()
            .AsNoTracking()
            .OrderBy(plataforma => plataforma.Id)
            .ToListAsync(cancellationToken);

        return Ok(plataformas);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Plataforma>> GetById(int id, CancellationToken cancellationToken)
    {
        var plataforma = await dbContext.Set<Plataforma>()
            .AsNoTracking()
            .SingleOrDefaultAsync(plataforma => plataforma.Id == id, cancellationToken);

        if (plataforma is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Plataforma não encontrada.");
        }

        return Ok(plataforma);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Plataforma>> Create(PlataformaInput input, CancellationToken cancellationToken)
    {
        if (!await dbContext.Estacoes.AnyAsync(estacao => estacao.Id == input.EstacaoId, cancellationToken))
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Estação não encontrada.");
        }

        var plataforma = new Plataforma
        {
            Nome = input.Nome,
            Sentido = input.Sentido,
            EstacaoId = input.EstacaoId,
            QuantidadePessoas = input.QuantidadePessoas,
            AtualizadoEm = input.QuantidadePessoas.HasValue ? DateTime.UtcNow : null
        };

        dbContext.Set<Plataforma>().Add(plataforma);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = plataforma.Id }, plataforma);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, PlataformaInput input, CancellationToken cancellationToken)
    {
        var plataforma = await dbContext.Set<Plataforma>().FindAsync(new object[] { id }, cancellationToken);
        if (plataforma is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Plataforma não encontrada.");
        }

        if (!await dbContext.Estacoes.AnyAsync(estacao => estacao.Id == input.EstacaoId, cancellationToken))
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Estação não encontrada.");
        }

        plataforma.Nome = input.Nome;
        plataforma.Sentido = input.Sentido;
        plataforma.EstacaoId = input.EstacaoId;
        plataforma.QuantidadePessoas = input.QuantidadePessoas;
        plataforma.AtualizadoEm = input.QuantidadePessoas.HasValue ? DateTime.UtcNow : null;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var plataforma = await dbContext.Set<Plataforma>().FindAsync(new object[] { id }, cancellationToken);
        if (plataforma is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Plataforma não encontrada.");
        }

        dbContext.Set<Plataforma>().Remove(plataforma);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 2292 })
        {
            return Problem(statusCode: 409, detail: "Existem paradas ou medições vinculadas. Remova esses registros antes de excluir este recurso.");
        }

        return NoContent();
    }
}
