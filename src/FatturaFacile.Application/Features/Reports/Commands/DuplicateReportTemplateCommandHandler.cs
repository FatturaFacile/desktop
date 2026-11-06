// <copyright file="DuplicateReportTemplateCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using MediatR;

namespace FatturaFacile.Application.Features.Reports.Commands;

/// <summary>
/// Handler for <see cref="DuplicateReportTemplateCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DuplicateReportTemplateCommandHandler"/> class.
/// </remarks>
/// <param name="templateStore">The template store.</param>
public sealed class DuplicateReportTemplateCommandHandler(IReportTemplateStore templateStore)
    : IRequestHandler<DuplicateReportTemplateCommand, ReportTemplateInfo>
{
    /// <inheritdoc/>
    public Task<ReportTemplateInfo> Handle(DuplicateReportTemplateCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return Task.FromResult(templateStore.Duplicate(request.DocumentType, request.SourceName, request.NewName));
    }
}
