using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

public record GetTranslationsQuery(string Locale) : IRequest<IReadOnlyList<AppTranslationDto>>;

public sealed class GetTranslationsQueryHandler(ILocalizationService localizationService) : IRequestHandler<GetTranslationsQuery, IReadOnlyList<AppTranslationDto>>
{
    private readonly ILocalizationService _localizationService = localizationService;

    public Task<IReadOnlyList<AppTranslationDto>> Handle(GetTranslationsQuery request, CancellationToken cancellationToken) => _localizationService.GetAllTranslationsAsync(request.Locale, cancellationToken);
}

public record UpdateTranslationCommand(
    string Locale,
    string Key,
    string CustomValue) : IRequest<Unit>;

public sealed class UpdateTranslationCommandHandler(ILocalizationService localizationService) : IRequestHandler<UpdateTranslationCommand, Unit>
{
    private readonly ILocalizationService _localizationService = localizationService;

    public async Task<Unit> Handle(UpdateTranslationCommand request, CancellationToken cancellationToken)
    {
        await _localizationService.SaveCustomTranslationAsync(request.Locale, request.Key, request.CustomValue, cancellationToken);
        return Unit.Value;
    }
}
