namespace Estoque.Domain.Entities;

public class MensagemProcessada
{
    public Guid Id { get; private set; }
    public string MessageId { get; private set; } = string.Empty;
    public DateTime DataProcessamento { get; private set; }

    protected MensagemProcessada()
    {
    }

    public MensagemProcessada(string messageId)
    {
        Id = Guid.NewGuid();
        MessageId = messageId;
        DataProcessamento = DateTime.UtcNow;
    }
}
