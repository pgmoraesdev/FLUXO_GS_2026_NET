using System.ComponentModel.DataAnnotations;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace Fluxo_GS_2026_2TDSPR.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicaoVagaoController : ControllerBase
{
    private readonly AppDbContext dbContext;

    public MedicaoVagaoController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    // Campos anuláveis permitem distinguir um valor ausente de uma contagem válida de zero.
    public record MedicaoVagaoInput(
        [Range(1, int.MaxValue)] int VagaoId,
        [Range(1, int.MaxValue)] int? ParadaProgramadaId,
        [Required, Range(0, int.MaxValue)] int? OcupacaoInicial,
        [Required, Range(0, int.MaxValue)] int? Embarques,
        [Required, Range(0, int.MaxValue)] int? Desembarques,
        [Required] DateTime? MedidaEm,
        [Required, StringLength(50)] string Origem) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (OcupacaoInicial.HasValue && Embarques.HasValue && Desembarques.HasValue)
            {
                // Usar long na soma evita estouro antes de verificar o limite do int.
                long ocupacao = (long)OcupacaoInicial.Value + Embarques.Value - Desembarques.Value;
                if (ocupacao < 0 || ocupacao > int.MaxValue)
                    yield return new ValidationResult("A ocupação calculada deve ficar entre zero e 2147483647 pessoas.",
                        new[] { nameof(OcupacaoInicial), nameof(Embarques), nameof(Desembarques) });
            }

            if (MedidaEm is { } data && (data == default || data.Kind != DateTimeKind.Utc || data > DateTime.UtcNow))
            {
                yield return new ValidationResult(
                    "Informe um horário UTC com sufixo Z, que não esteja no futuro.",
                    new[] { nameof(MedidaEm) });
            }
        }
    }

    // Oracle TIMESTAMP não guarda fuso. Todos os horários desta API são UTC.
    private static MedicaoVagao ComHorarioUtc(MedicaoVagao medicao)
    {
        medicao.MedidaEm = DateTime.SpecifyKind(medicao.MedidaEm, DateTimeKind.Utc);
        return medicao;
    }

    [HttpGet]
    public async Task<ActionResult<List<MedicaoVagao>>> GetAll(CancellationToken cancellationToken)
    {
        var medicoes = await dbContext.MedicoesVagao.AsNoTracking()
            .OrderByDescending(m => m.MedidaEm).ThenByDescending(m => m.Id)
            .ToListAsync(cancellationToken);

        return Ok(medicoes.Select(ComHorarioUtc).ToList());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicaoVagao>> GetById(int id, CancellationToken cancellationToken)
    {
        var medicao = await dbContext.MedicoesVagao.AsNoTracking()
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (medicao is null)
            return Problem(statusCode: 404, detail: "Medição não encontrada.");

        return Ok(ComHorarioUtc(medicao));
    }

    [HttpGet("/api/Vagao/{id}/ultima-ocupacao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicaoVagao>> GetUltimaOcupacao(int id, CancellationToken cancellationToken)
    {
        if (!await dbContext.Vagoes.AnyAsync(p => p.Id == id, cancellationToken))
            return Problem(statusCode: 404, detail: "Vagão não encontrado.");

        var agora = DateTime.UtcNow;
        var medicao = await dbContext.MedicoesVagao.AsNoTracking()
            .Where(m => m.VagaoId == id && m.MedidaEm <= agora)
            .OrderByDescending(m => m.MedidaEm).ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (medicao is null)
            return Problem(statusCode: 404, detail: "Nenhuma medição de ocupação encontrada para este vagão.");

        return Ok(ComHorarioUtc(medicao));
    }

    private async Task<ActionResult?> ValidarVinculos(MedicaoVagaoInput input, CancellationToken cancellationToken)
    {
        var vagao = await dbContext.Vagoes.AsNoTracking()
            .SingleOrDefaultAsync(v => v.Id == input.VagaoId, cancellationToken);
        if (vagao is null)
            return Problem(statusCode: 404, detail: "Vagão não encontrado.");

        if (input.ParadaProgramadaId.HasValue)
        {
            var parada = await dbContext.ParadasProgramadas.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == input.ParadaProgramadaId, cancellationToken);
            if (parada is null)
                return Problem(statusCode: 404, detail: "Parada programada não encontrada.");
            if (parada.TremId != vagao.TremId)
                return Problem(statusCode: 400, detail: "O vagão deve pertencer ao trem da parada informada.");
        }

        return null;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MedicaoVagao>> Create(MedicaoVagaoInput input, CancellationToken cancellationToken)
    {
        var erro = await ValidarVinculos(input, cancellationToken);
        if (erro is not null) return erro;

        var medicao = new MedicaoVagao
        {
            VagaoId = input.VagaoId,
            ParadaProgramadaId = input.ParadaProgramadaId,
            OcupacaoInicial = input.OcupacaoInicial!.Value,
            Embarques = input.Embarques!.Value,
            Desembarques = input.Desembarques!.Value,
            OcupacaoEstimada = (int)((long)input.OcupacaoInicial.Value + input.Embarques.Value - input.Desembarques.Value),
            MedidaEm = input.MedidaEm!.Value,
            Origem = input.Origem.Trim()
        };
        dbContext.MedicoesVagao.Add(medicao);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is OracleException { Number: 2291 })
        {
            return Problem(statusCode: 404, detail: "Vagão ou parada programada não encontrado.");
        }

        return CreatedAtAction(nameof(GetById), new { id = medicao.Id }, medicao);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, MedicaoVagaoInput input, CancellationToken cancellationToken)
    {
        var medicao = await dbContext.MedicoesVagao.FindAsync(new object[] { id }, cancellationToken);
        if (medicao is null)
            return Problem(statusCode: 404, detail: "Medição não encontrada.");

        var erro = await ValidarVinculos(input, cancellationToken);
        if (erro is not null) return erro;

        medicao.VagaoId = input.VagaoId;
        medicao.ParadaProgramadaId = input.ParadaProgramadaId;
        medicao.OcupacaoInicial = input.OcupacaoInicial!.Value;
        medicao.Embarques = input.Embarques!.Value;
        medicao.Desembarques = input.Desembarques!.Value;
        medicao.OcupacaoEstimada = (int)((long)medicao.OcupacaoInicial + medicao.Embarques - medicao.Desembarques);
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
            return Problem(statusCode: 404, detail: "Vagão ou parada programada não encontrado.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var medicao = await dbContext.MedicoesVagao.FindAsync(new object[] { id }, cancellationToken);
        if (medicao is null)
            return Problem(statusCode: 404, detail: "Medição não encontrada.");

        dbContext.MedicoesVagao.Remove(medicao);
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
