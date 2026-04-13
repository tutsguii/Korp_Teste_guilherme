using Estoque.Api.Contracts;
using FluentValidation;

namespace Estoque.Api.Validators;

public class CriarProdutoRequestValidator : AbstractValidator<CriarProdutoRequest>
{
    public CriarProdutoRequestValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("O codigo e obrigatorio.")
            .MaximumLength(50);

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("A descricao e obrigatoria.")
            .MaximumLength(200);

        RuleFor(x => x.Saldo)
            .GreaterThanOrEqualTo(0).WithMessage("O saldo deve ser maior ou igual a zero.");
    }
}
