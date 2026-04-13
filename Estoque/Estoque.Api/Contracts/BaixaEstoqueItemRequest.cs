namespace Estoque.Api.Contracts;

public class BaixaEstoqueItemRequest
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
