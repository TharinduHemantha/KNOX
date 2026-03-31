using FluentValidation;

namespace SecureMultiTenant.Application.Features.Auth.Commands.EntraExchange;

public sealed class EntraExchangeCommandValidator : AbstractValidator<EntraExchangeCommand>
{
    public EntraExchangeCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty()
            .MaximumLength(16384);
    }
}
