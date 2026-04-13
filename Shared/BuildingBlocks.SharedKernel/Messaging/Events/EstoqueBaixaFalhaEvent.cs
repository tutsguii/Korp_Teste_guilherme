namespace BuildingBlocks.SharedKernel.Messaging.Events;

public class EstoqueBaixaFalhaEvent
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; set; } = string.Empty;
    public Guid NotaFiscalId { get; set; }
    public string Message { get; set; } = string.Empty;
}
