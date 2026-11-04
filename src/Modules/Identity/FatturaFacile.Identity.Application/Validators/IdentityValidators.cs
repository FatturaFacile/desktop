using FluentValidation;
using FatturaFacile.Identity.Application.Commands;

namespace FatturaFacile.Identity.Application.Validators;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.TenantSlug).NotEmpty().WithMessage("TenantSlug is required.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("A valid email address is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public sealed class RegisterTenantUserCommandValidator : AbstractValidator<RegisterTenantUserCommand>
{
    public RegisterTenantUserCommandValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}
