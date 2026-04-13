namespace Faturamento.Api.Contracts;

public class AdicionarItemNotaFiscalRequest
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
