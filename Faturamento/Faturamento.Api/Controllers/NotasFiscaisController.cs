using BuildingBlocks.SharedKernel;
using BuildingBlocks.SharedKernel.Messaging;
using BuildingBlocks.SharedKernel.Messaging.Events;
using Faturamento.Api.Contracts;
using Faturamento.Domain.Entities;
using Faturamento.Domain.Enums;
using Faturamento.Infrastructure.Persistence;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Faturamento.Api.Controllers;

[ApiController]
[Route("api/notas")]
public class NotasFiscaisController : ControllerBase
{
    private readonly FaturamentoDbContext _context;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly ILogger<NotasFiscaisController> _logger;

    public NotasFiscaisController(
        FaturamentoDbContext context,
        IRabbitMqService rabbitMqService,
        ILogger<NotasFiscaisController> logger)
    {
        _context = context;
        _rabbitMqService = rabbitMqService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var notas = await _context.NotasFiscais
            .Include(x => x.Itens)
            .AsNoTracking()
            .OrderByDescending(x => x.Numero)
            .ToListAsync();

        return Ok(new ApiSuccessResponse<object>
        {
            Message = "Notas consultadas com sucesso.",
            Data = notas
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var nota = await _context.NotasFiscais
            .Include(x => x.Itens)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (nota == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Message = "Nota fiscal nao encontrada."
            });
        }

        return Ok(new ApiSuccessResponse<object>
        {
            Message = "Nota fiscal consultada com sucesso.",
            Data = nota
        });
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var nota = await _context.NotasFiscais
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (nota == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Message = "Nota fiscal nao encontrada."
            });
        }

        return Ok(new ApiSuccessResponse<object>
        {
            Message = "Status da nota consultado com sucesso.",
            Data = new
            {
                nota.Id,
                nota.Numero,
                Status = nota.Status.ToString(),
                nota.CorrelationId,
                nota.MensagemFalha
            }
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        NotaFiscal? nota = null;

        for (var tentativa = 1; tentativa <= 3; tentativa++)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var proximoNumero = await _context.NotasFiscais
                    .AsNoTracking()
                    .Select(x => (int?)x.Numero)
                    .MaxAsync() ?? 0;

                nota = new NotaFiscal(proximoNumero + 1);

                _context.NotasFiscais.Add(nota);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                break;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresEx &&
                                               postgresEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                                               tentativa < 3)
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();
            }
        }

        if (nota == null)
        {
            throw new DomainException("Nao foi possivel gerar a numeracao sequencial da nota.");
        }

        return CreatedAtAction(nameof(GetById), new { id = nota.Id }, new ApiSuccessResponse<object>
        {
            Message = "Nota fiscal criada com numeracao sequencial.",
            Data = nota
        });
    }

    [HttpPost("{id:guid}/itens")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AdicionarItemNotaFiscalRequest request)
    {
        var nota = await _context.NotasFiscais
            .Include(x => x.Itens)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (nota == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Message = "Nota fiscal nao encontrada."
            });
        }

        var item = nota.AdicionarItem(request.ProdutoId, request.Quantidade);
        _context.ItensNotaFiscal.Add(item);

        await _context.SaveChangesAsync();

        return Ok(new ApiSuccessResponse<object>
        {
            Message = "Item adicionado a nota com sucesso.",
            Data = nota
        });
    }

    [HttpPut("{id:guid}/itens/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid id, Guid itemId, [FromBody] AtualizarItemNotaFiscalRequest request)
    {
        var nota = await _context.NotasFiscais
            .Include(x => x.Itens)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (nota == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Message = "Nota fiscal nao encontrada."
            });
        }

        nota.AtualizarItem(itemId, request.ProdutoId, request.Quantidade);
        await _context.SaveChangesAsync();

        return Ok(new ApiSuccessResponse<object>
        {
            Message = "Item atualizado com sucesso.",
            Data = nota
        });
    }

    [HttpDelete("{id:guid}/itens/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id, Guid itemId)
    {
        var nota = await _context.NotasFiscais
            .Include(x => x.Itens)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (nota == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Message = "Nota fiscal nao encontrada."
            });
        }

        var item = nota.Itens.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Message = "Item da nota nao encontrado."
            });
        }

        nota.RemoverItem(itemId);
        _context.ItensNotaFiscal.Remove(item);
        await _context.SaveChangesAsync();

        return Ok(new ApiSuccessResponse<object>
        {
            Message = "Item removido com sucesso.",
            Data = nota
        });
    }

    [HttpPost("{id:guid}/fechamento")]
    public async Task<IActionResult> Fechar(Guid id)
    {
        _logger.LogInformation("Solicitando fechamento da nota {NotaFiscalId}", id);

        var nota = await _context.NotasFiscais
            .Include(x => x.Itens)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (nota == null)
        {
            return NotFound(new ApiErrorResponse
            {
                Message = "Nota fiscal nao encontrada."
            });
        }

        if (nota.Status != NotaFiscalStatus.Aberta)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Somente notas com status Aberta podem ser fechadas."
            });
        }

        if (nota.Itens.Count == 0)
        {
            return BadRequest(new ApiErrorResponse
            {
                Message = "Nao e possivel fechar uma nota sem itens."
            });
        }

        var correlationId = Guid.NewGuid().ToString();

        nota.IniciarFechamento();
        nota.DefinirCorrelationId(correlationId);
        await _context.SaveChangesAsync();

        var evento = new NotaFechamentoSolicitadoEvent
        {
            MessageId = Guid.NewGuid().ToString(),
            CorrelationId = correlationId,
            NotaFiscalId = nota.Id,
            Numero = nota.Numero,
            Itens = nota.Itens.Select(x => new NotaFechamentoSolicitadoItemEvent
            {
                ProdutoId = x.ProdutoId,
                Quantidade = x.Quantidade
            }).ToList()
        };

        _rabbitMqService.Publish("nota.fechamento.solicitado", evento);

        _logger.LogInformation(
            "Evento de fechamento publicado. NotaFiscalId: {NotaFiscalId}, CorrelationId: {CorrelationId}",
            nota.Id,
            correlationId);

        return Accepted(new ApiSuccessResponse<object>
        {
            Message = "Solicitacao de fechamento enviada para processamento.",
            Data = new
            {
                notaFiscalId = nota.Id
            }
        });
    }
}
