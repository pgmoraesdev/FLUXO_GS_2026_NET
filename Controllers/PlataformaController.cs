using System.ComponentModel.DataAnnotations;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Fluxo_GS_2026_2TDSPR.DTOs;
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

    [HttpGet("{id}/avaliar-capacidade")]
    public async Task<ActionResult<AvaliacaoCapacidadeResponse>> AvaliarCapacidade(int id,
        [FromQuery] int tremId, CancellationToken cancellationToken)
    {
        if (tremId <= 0)
            return Problem(statusCode: 400, detail: "Informe um tremId maior que zero.");

        var plataforma = await dbContext.Plataformas.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (plataforma is null)
            return Problem(statusCode: 404, detail: "Plataforma não encontrada.");
        if (!await dbContext.Trens.AnyAsync(t => t.Id == tremId, cancellationToken))
            return Problem(statusCode: 404, detail: "Trem não encontrado.");

        var agora = DateTime.UtcNow;
        if (plataforma.QuantidadePessoas is null or < 0 || plataforma.AtualizadoEm is null
            || plataforma.AtualizadoEm > agora || plataforma.AtualizadoEm < agora.AddMinutes(-5))
        {
            return Problem(statusCode: 400, detail: "Atualize a ocupação da plataforma. A leitura deve ter até cinco minutos.");
        }

        var vagoes = await dbContext.Vagoes.AsNoTracking()
            .Where(v => v.TremId == tremId).ToListAsync(cancellationToken);
        if (vagoes.Count == 0)
            return Problem(statusCode: 400, detail: "Cadastre os vagões do trem antes de avaliar a capacidade.");

        var lotacoes = vagoes.Select(v => LotacaoResponse.Criar(v, agora)).ToList();
        if (lotacoes.Any(l => l.OcupacaoEstimada is null))
            return Problem(statusCode: 400, detail: "Atualize a ocupação de todos os vagões. As leituras devem ter até cinco minutos.");

        long capacidadeDisponivel = lotacoes.Sum(l => Math.Max(0L,
            (long)l.CapacidadePessoas - l.OcupacaoEstimada!.Value));
        long pessoasSemEspaco = Math.Max(0L, plataforma.QuantidadePessoas.Value - capacidadeDisponivel);
        bool avaliarReforco = pessoasSemEspaco > 0;
        string mensagem = avaliarReforco
            ? "Capacidade insuficiente para as pessoas esperando. Avaliar reforço do serviço."
            : "O trem possui espaço para as pessoas esperando na plataforma.";

        return Ok(new AvaliacaoCapacidadeResponse(id, tremId, agora,
            plataforma.QuantidadePessoas.Value, capacidadeDisponivel, pessoasSemEspaco,
            avaliarReforco, mensagem));
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Plataforma>>> GetAll(CancellationToken cancellationToken)
    {
        var plataformas = await dbContext.Plataformas
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
        var plataforma = await dbContext.Plataformas
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

        Plataforma plataforma;
        try
        {
            plataforma = new Plataforma(input.Nome, input.Sentido, input.EstacaoId, input.QuantidadePessoas);
        }
        catch (ValidationException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: exception.Message);
        }

        dbContext.Plataformas.Add(plataforma);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = plataforma.Id }, plataforma);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, PlataformaInput input, CancellationToken cancellationToken)
    {
        var plataforma = await dbContext.Plataformas.FindAsync(new object[] { id }, cancellationToken);
        if (plataforma is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Plataforma não encontrada.");
        }

        if (!await dbContext.Estacoes.AnyAsync(estacao => estacao.Id == input.EstacaoId, cancellationToken))
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Estação não encontrada.");
        }

        try
        {
            plataforma.AtualizarCadastro(input.Nome, input.Sentido, input.EstacaoId, input.QuantidadePessoas);
        }
        catch (ValidationException exception)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: exception.Message);
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var plataforma = await dbContext.Plataformas.FindAsync(new object[] { id }, cancellationToken);
        if (plataforma is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, detail: "Plataforma não encontrada.");
        }

        dbContext.Plataformas.Remove(plataforma);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
