using System.ComponentModel.DataAnnotations;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace Fluxo_GS_2026_2TDSPR.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertaOperacionalController : ControllerBase
{
    private readonly AppDbContext dbContext;
    private const int ValidadeLeituraMinutos = 5;

    public AlertaOperacionalController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public record DesembarquePrevisto(
        [Range(1, int.MaxValue)] int VagaoId,
        [Required, Range(0, int.MaxValue)] int? QuantidadePessoas);

    public record AvaliacaoInput(
        [Required, MinLength(1)] List<DesembarquePrevisto>? DesembarquesPrevistos);

    public record AvaliacaoResponse(int ParadaProgramadaId, DateTime AvaliadoEm,
        int DemandaProjetada, long CapacidadeDisponivelEstimada, long PessoasPodemFicar,
        int? AlertaId, string? StatusAlerta);

    public record AtualizacaoAlertaInput(
        [Required] string Status,
        [StringLength(1000)] string? AcaoOperador) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Status != "Aberto" && Status != "EmAnalise" && Status != "Encerrado")
                yield return new ValidationResult("Use Aberto, EmAnalise ou Encerrado.", new[] { nameof(Status) });
            if (Status == "Encerrado" && string.IsNullOrWhiteSpace(AcaoOperador))
                yield return new ValidationResult("Registre a ação do operador antes de encerrar.", new[] { nameof(AcaoOperador) });
        }
    }

    private static AlertaOperacional ComHorarioUtc(AlertaOperacional alerta)
    {
        alerta.GeradoEm = DateTime.SpecifyKind(alerta.GeradoEm, DateTimeKind.Utc);
        if (alerta.AtualizadoEm.HasValue)
            alerta.AtualizadoEm = DateTime.SpecifyKind(alerta.AtualizadoEm.Value, DateTimeKind.Utc);
        return alerta;
    }

    [HttpGet]
    public async Task<ActionResult<List<AlertaOperacional>>> GetAll(CancellationToken cancellationToken)
    {
        var alertas = await dbContext.AlertasOperacionais.AsNoTracking()
            .OrderByDescending(a => a.GeradoEm).ThenByDescending(a => a.Id)
            .ToListAsync(cancellationToken);
        return Ok(alertas.Select(ComHorarioUtc).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertaOperacional>> GetById(int id, CancellationToken cancellationToken)
    {
        var alerta = await dbContext.AlertasOperacionais.AsNoTracking()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (alerta is null)
            return Problem(statusCode: 404, detail: "Alerta não encontrado.");
        return Ok(ComHorarioUtc(alerta));
    }

    [HttpPost("/api/ParadaProgramada/{id}/avaliar-demanda")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvaliacaoResponse>> AvaliarDemanda(int id, AvaliacaoInput input, CancellationToken cancellationToken)
    {
        var parada = await dbContext.ParadasProgramadas.AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (parada is null)
            return Problem(statusCode: 404, detail: "Parada programada não encontrada.");
        if (parada.ChegadaRealEm.HasValue)
            return Problem(statusCode: 400, detail: "A chegada desta parada já foi registrada.");

        var vagoes = await dbContext.Vagoes.AsNoTracking()
            .Where(v => v.TremId == parada.TremId).ToListAsync(cancellationToken);
        if (vagoes.Count == 0)
            return Problem(statusCode: 400, detail: "Cadastre os vagões do trem antes de avaliar a demanda.");

        var previsoes = input.DesembarquesPrevistos;
        if (previsoes is null || previsoes.Any(p => p is null)
            || previsoes.Count != vagoes.Count
            || previsoes.Select(p => p.VagaoId).Distinct().Count() != vagoes.Count
            || previsoes.Any(p => !vagoes.Any(v => v.Id == p.VagaoId)))
        {
            return Problem(statusCode: 400, detail: "Informe uma previsão de desembarque para cada vagão do trem, sem repetir IDs.");
        }

        var agora = DateTime.UtcNow;
        var limiteLeitura = agora.AddMinutes(-ValidadeLeituraMinutos);
        var plataforma = await dbContext.MedicoesPlataforma.AsNoTracking()
            .Where(m => m.PlataformaId == parada.PlataformaId && m.MedidaEm <= agora)
            .OrderByDescending(m => m.MedidaEm).ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (plataforma is null || plataforma.MedidaEm < limiteLeitura || plataforma.QuantidadePessoas < 0)
            return Problem(statusCode: 400, detail: "Registre uma medição válida da plataforma com até cinco minutos.");

        long capacidadeDisponivel = 0;
        foreach (var vagao in vagoes)
        {
            var medicao = await dbContext.MedicoesVagao.AsNoTracking()
                .Where(m => m.VagaoId == vagao.Id && m.MedidaEm <= agora)
                .OrderByDescending(m => m.MedidaEm).ThenByDescending(m => m.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (medicao is null || medicao.MedidaEm < limiteLeitura
                || medicao.OcupacaoEstimada < 0 || vagao.CapacidadePessoas <= 0)
            {
                return Problem(statusCode: 400, detail: $"Lotação indisponível para o vagão {vagao.Id}. Atualize sua medição.");
            }

            int desembarques = previsoes.Single(p => p.VagaoId == vagao.Id).QuantidadePessoas!.Value;
            if (desembarques < 0 || desembarques > medicao.OcupacaoEstimada)
                return Problem(statusCode: 400, detail: $"Os desembarques previstos do vagão {vagao.Id} devem ficar entre zero e sua ocupação.");

            // Desembarques previstos são futuros; não são os desembarques já contados na medição.
            long ocupacaoAposDesembarque = medicao.OcupacaoEstimada - desembarques;
            capacidadeDisponivel += Math.Max(0L, vagao.CapacidadePessoas - ocupacaoAposDesembarque);
        }

        int demanda = plataforma.QuantidadePessoas;
        long pessoasPodemFicar = Math.Max(0L, demanda - capacidadeDisponivel);
        var alerta = await dbContext.AlertasOperacionais.AsNoTracking()
            .SingleOrDefaultAsync(a => a.ParadaProgramadaId == id, cancellationToken);
        bool criado = false;

        if (pessoasPodemFicar > 0 && alerta is null)
        {
            alerta = new AlertaOperacional
            {
                ParadaProgramadaId = id, GeradoEm = agora,
                DemandaProjetada = demanda, CapacidadeDisponivelEstimada = capacidadeDisponivel
            };
            dbContext.AlertasOperacionais.Add(alerta);
            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                criado = true;
            }
            catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 1 })
            {
                // Outra requisição pode ter criado o alerta. O índice único impede duplicatas.
                alerta = await dbContext.AlertasOperacionais.AsNoTracking()
                    .SingleAsync(a => a.ParadaProgramadaId == id, cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 2291 })
            {
                return Problem(statusCode: 404, detail: "Parada programada não encontrada.");
            }
        }

        // A avaliação é atual; os números salvos no alerta continuam sendo os da abertura.
        var resposta = new AvaliacaoResponse(id, agora, demanda, capacidadeDisponivel,
            pessoasPodemFicar, alerta?.Id, alerta?.Status);
        if (criado)
            return CreatedAtAction(nameof(GetById), new { id = alerta!.Id }, resposta);
        return Ok(resposta);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, AtualizacaoAlertaInput input, CancellationToken cancellationToken)
    {
        var alerta = await dbContext.AlertasOperacionais.FindAsync(new object[] { id }, cancellationToken);
        if (alerta is null)
            return Problem(statusCode: 404, detail: "Alerta não encontrado.");

        alerta.Status = input.Status;
        alerta.AcaoOperador = string.IsNullOrWhiteSpace(input.AcaoOperador) ? null : input.AcaoOperador.Trim();
        alerta.AtualizadoEm = DateTime.UtcNow;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: 404, detail: "Alerta não encontrado.");
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var alerta = await dbContext.AlertasOperacionais.FindAsync(new object[] { id }, cancellationToken);
        if (alerta is null)
            return Problem(statusCode: 404, detail: "Alerta não encontrado.");
        dbContext.AlertasOperacionais.Remove(alerta);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Problem(statusCode: 404, detail: "Alerta não encontrado.");
        }
        return NoContent();
    }
}
