using FluentAssertions;
using Knox.Application.Features.Projects.Commands.CreateProject;
using Xunit;

namespace Knox.UnitTests;

public sealed class CreateProjectCommandValidatorTests
{
    [Fact]
    public void Should_fail_when_name_is_empty()
    {
        var validator = new CreateProjectCommandValidator();
        var result = validator.Validate(new CreateProjectCommand("", "PRJ-001", null));
        result.IsValid.Should().BeFalse();
    }
}
