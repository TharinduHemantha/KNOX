using FluentValidation;

namespace SecureMultiTenant.Application.Features.Auth.Commands.CreateEntraInvitation;

public sealed class CreateEntraInvitationCommandValidator : AbstractValidator<CreateEntraInvitationCommand>
{
    public CreateEntraInvitationCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.EntraTenantId).MaximumLength(128);
        RuleFor(x => x.RoleName).MaximumLength(256);
        RuleFor(x => x.ExpiryDays).InclusiveBetween(1, 30);
    }
}
