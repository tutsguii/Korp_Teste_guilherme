using System.Text;
using System.Text.Json;
using BuildingBlocks.SharedKernel.Messaging.Events;
using Estoque.Domain.Entities;
using Estoque.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Estoque.Api.Consumers;

public class NotaFechamentoSolicitadoConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotaFechamentoSolicitadoConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "korp.eventos";
    private const string QueueName = "estoque.nota.fechamento.solicitado";
    private const string RoutingKey = "nota.fechamento.solicitado";

    public NotaFechamentoSolicitadoConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<NotaFechamentoSolicitadoConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
        InitializeRabbitMq();
    }

    private void InitializeRabbitMq()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:HostName"],
            UserName = _configuration["RabbitMq:UserName"],
            Password = _configuration["RabbitMq:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, durable: true);

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();

            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            NotaFechamentoSolicitadoEvent? evento = null;

            try
            {
                evento = JsonSerializer.Deserialize<NotaFechamentoSolicitadoEvent>(json);

                _logger.LogInformation("Mensagem recebida para baixa de estoque.");

                if (evento == null || string.IsNullOrWhiteSpace(evento.MessageId))
                {
                    PublishFalha(new EstoqueBaixaFalhaEvent
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        CorrelationId = evento?.CorrelationId ?? string.Empty,
                        NotaFiscalId = evento?.NotaFiscalId ?? Guid.Empty,
                        Message = "Evento inválido para processamento de estoque."
                    });

                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                _logger.LogInformation(
                    "Processando baixa de estoque. NotaFiscalId: {NotaFiscalId}, CorrelationId: {CorrelationId}",
                    evento.NotaFiscalId,
                    evento.CorrelationId);

                var mensagemJaProcessada = await dbContext.MensagensProcessadas
                    .AsNoTracking()
                    .AnyAsync(x => x.MessageId == evento.MessageId, stoppingToken);

                if (mensagemJaProcessada)
                {
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                if (evento.Itens.Count == 0)
                {
                    PublishFalha(new EstoqueBaixaFalhaEvent
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        CorrelationId = evento.CorrelationId,
                        NotaFiscalId = evento.NotaFiscalId,
                        Message = "Evento inválido para processamento de estoque."
                    });

                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);

                foreach (var item in evento.Itens)
                {
                    var produto = await dbContext.Produtos
                        .FirstOrDefaultAsync(x => x.Id == item.ProdutoId, stoppingToken);

                    if (produto == null)
                    {
                        await transaction.RollbackAsync(stoppingToken);

                        PublishFalha(new EstoqueBaixaFalhaEvent
                        {
                            MessageId = Guid.NewGuid().ToString(),
                            CorrelationId = evento.CorrelationId,
                            NotaFiscalId = evento.NotaFiscalId,
                            Message = $"Produto {item.ProdutoId} não encontrado."
                        });

                        _channel!.BasicAck(ea.DeliveryTag, false);
                        return;
                    }

                    produto.BaixarEstoque(item.Quantidade);
                }

                dbContext.MensagensProcessadas.Add(new MensagemProcessada(evento.MessageId));

                await dbContext.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);

                PublishSucesso(new EstoqueBaixaSucessoEvent
                {
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = evento.CorrelationId,
                    NotaFiscalId = evento.NotaFiscalId,
                    Message = "Baixa de estoque realizada com sucesso."
                });

                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning(
                    "Falha de concorrência ao baixar estoque. NotaFiscalId: {NotaFiscalId}, CorrelationId: {CorrelationId}",
                    evento?.NotaFiscalId,
                    evento?.CorrelationId);

                PublishFalha(new EstoqueBaixaFalhaEvent
                {
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = evento?.CorrelationId ?? string.Empty,
                    NotaFiscalId = evento?.NotaFiscalId ?? Guid.Empty,
                    Message = "Conflito de concorrência ao atualizar o estoque."
                });

                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Falha ao baixar estoque da nota {NotaFiscalId}: {Message}",
                    evento?.NotaFiscalId,
                    ex.Message);

                PublishFalha(new EstoqueBaixaFalhaEvent
                {
                    MessageId = Guid.NewGuid().ToString(),
                    CorrelationId = evento?.CorrelationId ?? string.Empty,
                    NotaFiscalId = evento?.NotaFiscalId ?? Guid.Empty,
                    Message = $"Erro ao processar estoque: {ex.Message}"
                });

                _channel!.BasicAck(ea.DeliveryTag, false);
            }
        };

        _channel!.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    private void PublishSucesso(EstoqueBaixaSucessoEvent message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        var properties = _channel!.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: "estoque.baixa.sucesso",
            basicProperties: properties,
            body: body);
    }

    private void PublishFalha(EstoqueBaixaFalhaEvent message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);
        var properties = _channel!.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: "estoque.baixa.falha",
            basicProperties: properties,
            body: body);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
