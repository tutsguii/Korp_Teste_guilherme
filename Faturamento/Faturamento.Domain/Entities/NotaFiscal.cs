using BuildingBlocks.SharedKernel;
using Faturamento.Domain.Enums;

namespace Faturamento.Domain.Entities;

public class NotaFiscal
{
    public Guid Id { get; private set; }
    public int Numero { get; private set; }
    public NotaFiscalStatus Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? MensagemFalha { get; private set; }
    public List<ItemNotaFiscal> Itens { get; private set; } = new();

    protected NotaFiscal()
    {
    }

    public NotaFiscal(int numero)
    {
        if (numero <= 0)
        {
            throw new DomainException("Numero da nota invalido.");
        }

        Id = Guid.NewGuid();
        Numero = numero;
        Status = NotaFiscalStatus.Aberta;
        DataCriacao = DateTime.UtcNow;
    }

    public ItemNotaFiscal AdicionarItem(Guid produtoId, int quantidade)
    {
        ValidarNotaAbertaParaEdicao();

        var item = new ItemNotaFiscal(Id, produtoId, quantidade);
        Itens.Add(item);
        return item;
    }

    public void AtualizarItem(Guid itemId, Guid produtoId, int quantidade)
    {
        ValidarNotaAbertaParaEdicao();

        var item = Itens.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
        {
            throw new DomainException("Item da nota nao encontrado.");
        }

        item.Atualizar(produtoId, quantidade);
    }

    public void RemoverItem(Guid itemId)
    {
        ValidarNotaAbertaParaEdicao();

        var item = Itens.FirstOrDefault(x => x.Id == itemId);
        if (item == null)
        {
            throw new DomainException("Item da nota nao encontrado.");
        }

        Itens.Remove(item);
    }

    public void IniciarFechamento()
    {
        if (Status != NotaFiscalStatus.Aberta)
        {
            throw new DomainException("A nota precisa estar aberta para iniciar o fechamento.");
        }

        MensagemFalha = null;
        Status = NotaFiscalStatus.ProcessandoFechamento;
    }

    public void DefinirCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
    }

    public void MarcarFechada()
    {
        if (Status != NotaFiscalStatus.ProcessandoFechamento)
        {
            throw new DomainException("A nota precisa estar processando para ser fechada.");
        }

        MensagemFalha = null;
        Status = NotaFiscalStatus.Fechada;
    }

    public void MarcarErroAoFechar(string? mensagemFalha = null)
    {
        if (Status != NotaFiscalStatus.ProcessandoFechamento)
        {
            throw new DomainException("A nota precisa estar processando para registrar erro.");
        }

        MensagemFalha = mensagemFalha;
        Status = NotaFiscalStatus.ErroAoFechar;
    }

    public void VoltarParaAberta()
    {
        if (Status != NotaFiscalStatus.ProcessandoFechamento &&
            Status != NotaFiscalStatus.ErroAoFechar)
        {
            throw new DomainException("A nota nao pode voltar para aberta no estado atual.");
        }

        Status = NotaFiscalStatus.Aberta;
    }

    private void ValidarNotaAbertaParaEdicao()
    {
        if (Status != NotaFiscalStatus.Aberta)
        {
            throw new DomainException("So e permitido alterar itens em nota aberta.");
        }
    }
}
