using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fluxo_GS_2026_2TDSPR.Controllers;

[ApiController]
[Route("api")]
public class LotacaoController : ControllerBase
{
    private readonly AppDbContext dbContext;

    // Valores escolhidos para a simulação acadêmica, não são limites operacionais.
    private const int ValidadeLeituraMinutos = 5;

    public LotacaoController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public record LotacaoResponse(
        int VagaoId,
        int Posicao,
        int CapacidadePessoas,
        int? OcupacaoEstimada,
        decimal? PercentualOcupacao,
        DateTime? MedidaEm,
        string Situacao,
        string Cor);

    [HttpGet("Vagao/{id}/lotacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LotacaoResponse>> GetVagao(int id, CancellationToken cancellationToken)
    {
        var vagao = await dbContext.Vagoes.AsNoTracking()
            .SingleOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (vagao is null)
            return Problem(statusCode: 404, detail: "Vagão não encontrado.");

        return Ok(await ConsultarLotacao(vagao, DateTime.UtcNow, cancellationToken));
    }

    [HttpGet("Trem/{id}/lotacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<LotacaoResponse>>> GetTrem(int id, CancellationToken cancellationToken)
    {
        if (!await dbContext.Trens.AnyAsync(t => t.Id == id, cancellationToken))
            return Problem(statusCode: 404, detail: "Trem não encontrado.");

        var vagoes = await dbContext.Vagoes.AsNoTracking()
            .Where(v => v.TremId == id).OrderBy(v => v.Posicao)
            .ToListAsync(cancellationToken);

        var resultado = new List<LotacaoResponse>();
        var agora = DateTime.UtcNow;
        foreach (var vagao in vagoes)
            resultado.Add(await ConsultarLotacao(vagao, agora, cancellationToken));

        return Ok(resultado);
    }

    private async Task<LotacaoResponse> ConsultarLotacao(Vagao vagao, DateTime agora, CancellationToken cancellationToken)
    {
        var medicao = await dbContext.MedicoesVagao.AsNoTracking()
            .Where(m => m.VagaoId == vagao.Id && m.MedidaEm <= agora)
            .OrderByDescending(m => m.MedidaEm).ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        DateTime? medidaEm = medicao is null
            ? null
            : DateTime.SpecifyKind(medicao.MedidaEm, DateTimeKind.Utc);

        if (medicao is null || medicao.MedidaEm < agora.AddMinutes(-ValidadeLeituraMinutos)
            || medicao.OcupacaoEstimada < 0 || vagao.CapacidadePessoas <= 0)
        {
            // Sem leitura recente, não apresentar uma contagem antiga como atual.
            return new LotacaoResponse(vagao.Id, vagao.Posicao, vagao.CapacidadePessoas,
                null, null, medidaEm, "Lotação indisponível", "Cinza");
        }

        decimal percentual = 100m * medicao.OcupacaoEstimada / vagao.CapacidadePessoas;
        string situacao;
        string cor;

        if (percentual < 50)
        {
            situacao = "Com espaço";
            cor = "Verde";
        }
        else if (percentual < 80)
        {
            situacao = "Ocupado";
            cor = "Amarelo";
        }
        else
        {
            situacao = "Muito cheio";
            cor = "Vermelho";
        }

        return new LotacaoResponse(vagao.Id, vagao.Posicao, vagao.CapacidadePessoas,
            medicao.OcupacaoEstimada, percentual, medidaEm, situacao, cor);
    }
}
