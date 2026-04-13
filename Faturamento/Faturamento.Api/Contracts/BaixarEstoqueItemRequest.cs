namespace Faturamento.Api.Contracts;

public class BaixarEstoqueItemRequest
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
