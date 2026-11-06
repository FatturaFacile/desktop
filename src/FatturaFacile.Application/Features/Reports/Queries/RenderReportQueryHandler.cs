// <copyright file="RenderReportQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Application.Features.Reports.Queries;

/// <summary>
/// Handler for <see cref="RenderReportQuery"/>: only templates listed in the folder of the document type can be used.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RenderReportQueryHandler"/> class.
/// </remarks>
/// <param name="templateStore">The template store.</param>
/// <param name="renderer">The renderer.</param>
/// <param name="builders">The data builders of all document types.</param>
/// <param name="currentTenantService">The current company.</param>
public sealed class RenderReportQueryHandler(
    IReportTemplateStore templateStore,
    IReportRenderer renderer,
    IEnumerable<IReportDataBuilder> builders,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<RenderReportQuery, RenderedReport>
{
    /// <inheritdoc/>
    public Task<RenderedReport> Handle(RenderReportQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var template = templateStore.GetTemplates(request.DocumentType)
            .FirstOrDefault(t => string.Equals(t.Name, request.TemplateName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Il modello '{request.TemplateName}' non esiste più nella cartella dei modelli.");
        var builder = builders.FirstOrDefault(b => b.DocumentType == request.DocumentType);

        return builder is null
            ? throw new NotSupportedException($"Stampa non disponibile per {request.DocumentType}.")
            : HandleInternalAsync(request, template, builder, cancellationToken);
    }

    private async Task<RenderedReport> HandleInternalAsync(
        RenderReportQuery request,
        ReportTemplateInfo template,
        IReportDataBuilder builder,
        CancellationToken cancellationToken)
    {
        var culture = ReportCulture.FromTemplateName(template.Name, currentTenantService.CountryCode);
        var pending = builder is IPeriodReportDataBuilder periodBuilder && request.From is { } from && request.To is { } to
            ? periodBuilder.BuildAsync(request.EntityId, from, to, culture, cancellationToken)
            : builder.BuildAsync(request.EntityId, culture, cancellationToken);
        var data = await pending.ConfigureAwait(false)
            ?? throw new InvalidOperationException("Il documento da stampare non esiste.");

        var pdf = await renderer.RenderPdfAsync(template.FilePath, data, cancellationToken).ConfigureAwait(false);
        return new RenderedReport(pdf, data.SuggestedFileName, request.DocumentType);
    }
}
