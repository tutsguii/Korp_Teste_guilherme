namespace Faturamento.Api.Contracts;

public class BaixarEstoqueRequest
{
    public Guid NotaFiscalId { get; set; }
    public List<BaixarEstoqueItemRequest> Itens { get; set; } = new();
}
