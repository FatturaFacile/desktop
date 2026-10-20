using FatturaFacile.Domain.Enums;
using FluentValidation;

namespace FatturaFacile.Application.Features.Clients;

/// <summary>
/// Represents create client command validator.
/// </summary>
public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateClientCommandValidator"/> class.
    /// </summary>
    public CreateClientCommandValidator()
    {
        _ = RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        _ = RuleFor(x => x.Email).NotEmpty().EmailAddress();
        _ = RuleFor(x => x.Street).NotEmpty();
        _ = RuleFor(x => x.City).NotEmpty();
        _ = RuleFor(x => x.PostalCode).NotEmpty();

        _ = When(x => x.Jurisdiction == Jurisdiction.Italy, () =>
        {
            _ = RuleFor(x => x.FiscalCode).NotEmpty();
            _ = RuleFor(x => x.VatNumber).NotEmpty();
        });

        _ = When(x => x.Jurisdiction == Jurisdiction.Switzerland, () => _ = RuleFor(x => x.SwissUid).NotEmpty());
    }
}
