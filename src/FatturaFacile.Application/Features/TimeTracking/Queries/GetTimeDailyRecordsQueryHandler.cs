// <copyright file="GetTimeDailyRecordsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Handler for <see cref="GetTimeDailyRecordsQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetTimeDailyRecordsQueryHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="access">The working time access rules.</param>
public sealed class GetTimeDailyRecordsQueryHandler(IUnitOfWork unitOfWork, TimeAccess access) : IRequestHandler<GetTimeDailyRecordsQuery, IReadOnlyList<TimeDailyRecord>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<TimeDailyRecord>> Handle(GetTimeDailyRecordsQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<IReadOnlyList<TimeDailyRecord>> HandleInternalAsync(GetTimeDailyRecordsQuery request, CancellationToken cancellationToken)
    {
        await access.EnsureCanReadAsync(request.EmployeeId, cancellationToken).ConfigureAwait(false);

        var employeeId = request.EmployeeId;
        var from = request.From.Date;
        var to = request.To.Date;
        var records = await unitOfWork.Repository<TimeDailyRecord>()
            .FindAsync(r => r.EmployeeId == employeeId && r.Date >= from && r.Date <= to, cancellationToken)
            .ConfigureAwait(false);

        return [.. records.OrderBy(static r => r.Date).Select(RecordCopier.Clone)];
    }
}
