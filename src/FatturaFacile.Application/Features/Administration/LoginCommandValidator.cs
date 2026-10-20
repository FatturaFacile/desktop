using FluentValidation;

namespace FatturaFacile.Application.Features.Administration;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        _ = RuleFor(x => x.Username).NotEmpty();
        _ = RuleFor(x => x.Password).NotEmpty();
        _ = RuleFor(x => x.TenantId).NotEmpty();
    }
}