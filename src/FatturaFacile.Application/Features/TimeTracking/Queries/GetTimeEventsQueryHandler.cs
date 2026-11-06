// <copyright file="GetTimeEventsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Handler for <see cref="GetTimeEventsQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetTimeEventsQueryHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="access">The working time access rules.</param>
public sealed class GetTimeEventsQueryHandler(IUnitOfWork unitOfWork, TimeAccess access) : IRequestHandler<GetTimeEventsQuery, IReadOnlyList<TimeClockEvent>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<TimeClockEvent>> Handle(GetTimeEventsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<IReadOnlyList<TimeClockEvent>> HandleInternalAsync(GetTimeEventsQuery request, CancellationToken cancellationToken)
    {
        await access.EnsureCanReadAsync(request.EmployeeId, cancellationToken).ConfigureAwait(false);

        var employeeId = request.EmployeeId;
        var from = request.From.Date;
        var to = request.To.Date.AddDays(1);
        var events = await unitOfWork.Repository<TimeClockEvent>()
            .FindAsync(e => e.EmployeeId == employeeId && e.Timestamp >= from && e.Timestamp < to, cancellationToken)
            .ConfigureAwait(false);

        return [.. events.Where(e => request.IncludeDeleted || e.DeletedAt is null).OrderBy(static e => e.Timestamp).Select(RecordCopier.Clone)];
    }
}
