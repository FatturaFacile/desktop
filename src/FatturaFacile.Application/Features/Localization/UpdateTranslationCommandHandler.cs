using FatturaFacile.Application.Common.Interfaces;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

public sealed class UpdateTranslationCommandHandler(ILocalizationService localizationService) : IRequestHandler<UpdateTranslationCommand, Unit>
{
    private readonly ILocalizationService _localizationService = localizationService;

    public async Task<Unit> Handle(UpdateTranslationCommand request, CancellationToken cancellationToken)
    {
        await _localizationService.SaveCustomTranslationAsync(request.Locale, request.Key, request.CustomValue, cancellationToken);
        return Unit.Value;
    }
}