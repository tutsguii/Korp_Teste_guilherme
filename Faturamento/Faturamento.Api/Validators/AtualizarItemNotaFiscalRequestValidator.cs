using Faturamento.Api.Contracts;
using FluentValidation;

namespace Faturamento.Api.Validators;

public class AtualizarItemNotaFiscalRequestValidator : AbstractValidator<AtualizarItemNotaFiscalRequest>
{
    public AtualizarItemNotaFiscalRequestValidator()
    {
        RuleFor(x => x.ProdutoId)
            .NotEmpty().WithMessage("O produto e obrigatorio.");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
    }
}
