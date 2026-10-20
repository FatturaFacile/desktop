using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

public sealed class GetTranslationsQueryHandler(ILocalizationService localizationService) : IRequestHandler<GetTranslationsQuery, IReadOnlyList<AppTranslationDto>>
{
    private readonly ILocalizationService _localizationService = localizationService;

    public Task<IReadOnlyList<AppTranslationDto>> Handle(GetTranslationsQuery request, CancellationToken cancellationToken) => _localizationService.GetAllTranslationsAsync(request.Locale, cancellationToken);
}