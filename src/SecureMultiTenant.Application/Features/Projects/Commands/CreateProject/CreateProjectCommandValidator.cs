using FluentValidation;

namespace SecureMultiTenant.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50).Matches(@"^[a-zA-Z0-9\s\-_\.]+$");
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
