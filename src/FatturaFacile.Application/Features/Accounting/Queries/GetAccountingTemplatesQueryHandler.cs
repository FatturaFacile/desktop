// <copyright file="GetAccountingTemplatesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Accounting;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Accounting;
using MediatR;

namespace FatturaFacile.Application.Features.Accounting.Queries;

/// <summary>
/// Lists the cached templates in the language of the user, completed with the built-in ones.
/// </summary>
/// <param name="repository">The cached templates.</param>
/// <param name="localization">The localization service.</param>
public sealed class GetAccountingTemplatesQueryHandler(IRepository<AccountingTemplate> repository, ILocalizationService localization) : IRequestHandler<GetAccountingTemplatesQuery, IReadOnlyList<TemplateDefinition>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<TemplateDefinition>> Handle(GetAccountingTemplatesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var language = localization.CurrentCulture.TwoLetterISOLanguageName;
        var synced = repository.Query().OrderBy(t => t.Code).ToList().ConvertAll(t => TemplateParser.ToDefinition(t, language));
        var codes = synced.Select(static t => t.Code).ToHashSet(StringComparer.Ordinal);

        IReadOnlyList<TemplateDefinition> all = [.. synced, .. BuiltInTemplates.All(language).Where(t => !codes.Contains(t.Code))];

        return Task.FromResult(all);
    }
}
