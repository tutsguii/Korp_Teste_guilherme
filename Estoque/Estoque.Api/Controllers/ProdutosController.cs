using BuildingBlocks.SharedKernel;
using Estoque.Api.Contracts;
using Estoque.Domain.Entities;
using Estoque.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly EstoqueDbContext _context;

    public ProdutosController(EstoqueDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var produtos = await _context.Produtos
            .AsNoTracking()
            .OrderBy(x => x.Descricao)
            .ToListAsync();

        return Ok(new ApiSuccessResponse<object>
        {
            Message = "Produtos consultados com sucesso.",
            Data = produtos
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var produto = await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (produto == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Message = "Produto não encontrado."
            });
        }

        return Ok(new ApiSuccessResponse<object>
        {
            Message = "Produto consultado com sucesso.",
            Data = produto
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarProdutoRequest request)
    {
        var produto = new Produto(request.Codigo, request.Descricao, request.Saldo);

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = produto.Id }, new ApiSuccessResponse<object>
        {
            Message = "Produto criado com sucesso.",
            Data = produto
        });
    }
}
