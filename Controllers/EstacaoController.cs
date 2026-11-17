
using Microsoft.AspNetCore.Http;
using Fluxo_GS_2026_2TDSPR.Data;
using Fluxo_GS_2026_2TDSPR.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fluxo_GS_2026_2TDSPR.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstacaoController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public EstacaoController(AppDbContext _dbContext)
        {
            dbContext = _dbContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            var estacoes = await dbContext.Estacoes.ToListAsync();

            if (estacoes == null || estacoes.Count == 0)
            {
                return NotFound("Nenhuma estação encontrada.");
            }

            return Ok(estacoes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var estacao = await dbContext.Estacoes.FindAsync(id);

            if (estacao == null)
            {
                return NotFound("Estação não encontrada.");
            }

            return Ok(estacao);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(Estacao estacaoToSave)
        {
            dbContext.Estacoes.Add(estacaoToSave);

            await dbContext.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = estacaoToSave.Id },
                estacaoToSave
            );
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, Estacao estacaoToUpdate)
        {
            var estacaoToChange = await dbContext.Estacoes.FindAsync(id);

            if (estacaoToChange == null)
            {
                return NotFound("Estação não encontrada.");
            }

            estacaoToChange.Nome = estacaoToUpdate.Nome;

            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var estacao = await dbContext.Estacoes.FindAsync(id);

            if (estacao == null)
            {
                return NotFound("Estação não encontrada.");
            }

            dbContext.Estacoes.Remove(estacao);

            await dbContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
