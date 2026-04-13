using System.Text;
using System.Text.Json;
using BuildingBlocks.SharedKernel.Messaging.Events;
using Faturamento.Domain.Entities;
using Faturamento.Domain.Enums;
using Faturamento.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Faturamento.Api.Consumers;

public class EstoqueBaixaFalhaConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EstoqueBaixaFalhaConsumer> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "korp.eventos";
    private const string QueueName = "faturamento.estoque.baixa.falha";
    private const string RoutingKey = "estoque.baixa.falha";

    public EstoqueBaixaFalhaConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<EstoqueBaixaFalhaConsumer> logger)
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
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, ExchangeName, RoutingKey);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FaturamentoDbContext>();

            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            try
            {
                var evento = JsonSerializer.Deserialize<EstoqueBaixaFalhaEvent>(json);

                _logger.LogInformation("Mensagem de falha de estoque recebida.");

                if (evento == null || string.IsNullOrWhiteSpace(evento.MessageId))
                {
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var mensagemJaProcessada = await dbContext.MensagensProcessadas
                    .AsNoTracking()
                    .AnyAsync(x => x.MessageId == evento.MessageId, stoppingToken);

                if (mensagemJaProcessada)
                {
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var nota = await dbContext.NotasFiscais
                    .FirstOrDefaultAsync(x => x.Id == evento.NotaFiscalId, stoppingToken);

                if (nota != null &&
                    nota.CorrelationId == evento.CorrelationId &&
                    nota.Status == NotaFiscalStatus.ProcessandoFechamento)
                {
                    _logger.LogWarning(
                        "Falha ao fechar nota {NotaFiscalId}. CorrelationId: {CorrelationId}. Mensagem: {Mensagem}",
                        evento.NotaFiscalId,
                        evento.CorrelationId,
                        evento.Message);

                    nota.MarcarErroAoFechar(evento.Message);
                    nota.VoltarParaAberta();

                    dbContext.MensagensProcessadas.Add(new MensagemProcessada(evento.MessageId));
                    await dbContext.SaveChangesAsync(stoppingToken);
                }

                _channel!.BasicAck(ea.DeliveryTag, false);
            }
            catch
            {
                _channel!.BasicAck(ea.DeliveryTag, false);
            }
        };

        _channel!.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
