using Microsoft.AspNetCore.Http;
using Fluxo_GS_2026_2TDSPR.Data;
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
    }
}
