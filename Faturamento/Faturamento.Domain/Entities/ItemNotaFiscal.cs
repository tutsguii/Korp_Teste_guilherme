using BuildingBlocks.SharedKernel;

namespace Faturamento.Domain.Entities;

public class ItemNotaFiscal
{
    public Guid Id { get; private set; }
    public Guid ProdutoId { get; private set; }
    public int Quantidade { get; private set; }
    public Guid NotaFiscalId { get; private set; }

    protected ItemNotaFiscal()
    {
    }

    public ItemNotaFiscal(Guid notaFiscalId, Guid produtoId, int quantidade)
    {
        if (notaFiscalId == Guid.Empty)
        {
            throw new DomainException("Nota fiscal invalida.");
        }

        if (produtoId == Guid.Empty)
        {
            throw new DomainException("Produto invalido.");
        }

        if (quantidade <= 0)
        {
            throw new DomainException("Quantidade invalida.");
        }

        Id = Guid.NewGuid();
        NotaFiscalId = notaFiscalId;
        ProdutoId = produtoId;
        Quantidade = quantidade;
    }

    public void Atualizar(Guid produtoId, int quantidade)
    {
        if (produtoId == Guid.Empty)
        {
            throw new DomainException("Produto invalido.");
        }

        if (quantidade <= 0)
        {
            throw new DomainException("Quantidade invalida.");
        }

        ProdutoId = produtoId;
        Quantidade = quantidade;
    }
}
