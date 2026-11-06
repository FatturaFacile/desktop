// <copyright file="ArchiveReportCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Application.Features.Reports.Commands;

/// <summary>
/// Handler for <see cref="ArchiveReportCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ArchiveReportCommandHandler"/> class.
/// </remarks>
/// <param name="archive">The document archive.</param>
/// <param name="currentTenantService">The current company.</param>
public sealed class ArchiveReportCommandHandler(IDocumentArchive archive, ICurrentTenantService currentTenantService)
    : IRequestHandler<ArchiveReportCommand, string>
{
    /// <inheritdoc/>
    public Task<string> Handle(ArchiveReportCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var report = request.Report;
        return archive.SaveAsync(currentTenantService.TenantName, report.DocumentType, report.FileName, report.Content.ToArray(), cancellationToken);
    }
}
