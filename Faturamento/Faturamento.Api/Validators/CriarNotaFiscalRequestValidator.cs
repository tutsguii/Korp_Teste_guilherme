using Faturamento.Api.Contracts;
using FluentValidation;

namespace Faturamento.Api.Validators;

public class CriarNotaFiscalRequestValidator : AbstractValidator<CriarNotaFiscalRequest>
{
    public CriarNotaFiscalRequestValidator()
    {
    }
}
