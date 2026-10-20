using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Localization;

public record GetTranslationsQuery(string Locale) : IRequest<IReadOnlyList<AppTranslationDto>>;