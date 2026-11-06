// <copyright file="GetTimeAuditQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Handler for <see cref="GetTimeAuditQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetTimeAuditQueryHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetTimeAuditQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTimeAuditQuery, IReadOnlyList<TimeAuditEntry>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<TimeAuditEntry>> Handle(GetTimeAuditQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<IReadOnlyList<TimeAuditEntry>> HandleInternalAsync(GetTimeAuditQuery request, CancellationToken cancellationToken)
    {
        var from = request.From.Date;
        var to = request.To.Date.AddDays(1);
        var entries = await unitOfWork.Repository<TimeAuditEntry>()
            .FindAsync(a => a.Timestamp >= from && a.Timestamp < to, cancellationToken)
            .ConfigureAwait(false);

        var filtered = entries
            .Where(a => request.EmployeeId is null || a.EmployeeId == request.EmployeeId)
            .OrderByDescending(static a => a.Timestamp)
            .Select(RecordCopier.Clone);

        return [.. filtered];
    }
}
