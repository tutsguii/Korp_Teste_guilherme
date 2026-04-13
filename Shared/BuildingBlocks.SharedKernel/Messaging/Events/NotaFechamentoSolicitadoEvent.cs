namespace BuildingBlocks.SharedKernel.Messaging.Events;

public class NotaFechamentoSolicitadoEvent
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public Guid NotaFiscalId { get; set; }
    public int Numero { get; set; }
    public List<NotaFechamentoSolicitadoItemEvent> Itens { get; set; } = new();
}

public class NotaFechamentoSolicitadoItemEvent
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
