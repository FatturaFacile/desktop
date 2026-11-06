// <copyright file="GetReportTemplatesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using MediatR;

namespace FatturaFacile.Application.Features.Reports.Queries;

/// <summary>
/// Handler for <see cref="GetReportTemplatesQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetReportTemplatesQueryHandler"/> class.
/// </remarks>
/// <param name="templateStore">The template store.</param>
public sealed class GetReportTemplatesQueryHandler(IReportTemplateStore templateStore)
    : IRequestHandler<GetReportTemplatesQuery, IReadOnlyList<ReportTemplateInfo>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<ReportTemplateInfo>> Handle(GetReportTemplatesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Task.FromResult(templateStore.GetTemplates(request.DocumentType));
    }
}
