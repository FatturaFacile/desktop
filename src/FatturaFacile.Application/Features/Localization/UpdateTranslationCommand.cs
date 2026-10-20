using MediatR;

namespace FatturaFacile.Application.Features.Localization;

public record UpdateTranslationCommand(
    string Locale,
    string Key,
    string CustomValue) : IRequest<Unit>;