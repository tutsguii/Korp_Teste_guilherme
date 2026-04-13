namespace Faturamento.Api.Contracts;

public class AtualizarItemNotaFiscalRequest
{
    public Guid ProdutoId { get; set; }
    public int Quantidade { get; set; }
}
