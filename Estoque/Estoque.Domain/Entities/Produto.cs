using BuildingBlocks.SharedKernel;

namespace Estoque.Domain.Entities;

public class Produto
{
    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public int Saldo { get; private set; }

    protected Produto()
    {
    }

    public Produto(string codigo, string descricao, int saldo)
    {
        Id = Guid.NewGuid();
        DefinirCodigo(codigo);
        DefinirDescricao(descricao);
        DefinirSaldoInicial(saldo);
    }

    public void DefinirCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new DomainException("O codigo do produto e obrigatorio.");
        }

        Codigo = codigo.Trim();
    }

    public void DefinirDescricao(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new DomainException("A descricao do produto e obrigatoria.");
        }

        Descricao = descricao.Trim();
    }

    public void DefinirSaldoInicial(int saldo)
    {
        if (saldo < 0)
        {
            throw new DomainException("O saldo nao pode ser negativo.");
        }

        Saldo = saldo;
    }

    public void BaixarEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new DomainException("Quantidade invalida para baixa de estoque.");
        }

        if (Saldo < quantidade)
        {
            throw new DomainException("Saldo insuficiente.");
        }

        Saldo -= quantidade;
    }
}
