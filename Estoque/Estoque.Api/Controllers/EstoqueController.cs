using Estoque.Api.Contracts;
using Estoque.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Controllers;

[ApiController]
[Route("api/estoque")]
public class EstoqueController : ControllerBase
{
    private readonly EstoqueDbContext _context;

    public EstoqueController(EstoqueDbContext context)
    {
        _context = context;
    }

    [HttpPost("baixar")]
    public async Task<IActionResult> Baixar([FromBody] BaixaEstoqueRequest request)
    {
        if (request.Itens == null || request.Itens.Count == 0)
        {
            return BadRequest(new BaixaEstoqueResponse
            {
                Success = false,
                Message = "Nenhum item informado para baixa de estoque."
            });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in request.Itens)
            {
                var produto = await _context.Produtos
                    .FirstOrDefaultAsync(x => x.Id == item.ProdutoId);

                if (produto == null)
                {
                    await transaction.RollbackAsync();

                    return NotFound(new BaixaEstoqueResponse
                    {
                        Success = false,
                        Message = $"Produto {item.ProdutoId} não encontrado."
                    });
                }

                produto.BaixarEstoque(item.Quantidade);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new BaixaEstoqueResponse
            {
                Success = true,
                Message = "Baixa de estoque realizada com sucesso."
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return BadRequest(new BaixaEstoqueResponse
            {
                Success = false,
                Message = $"Erro ao baixar estoque: {ex.Message}"
            });
        }
    }
}
