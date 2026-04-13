namespace Estoque.Api.Contracts;

public class BaixaEstoqueRequest
{
    public Guid NotaFiscalId { get; set; }
    public List<BaixaEstoqueItemRequest> Itens { get; set; } = new();
}
