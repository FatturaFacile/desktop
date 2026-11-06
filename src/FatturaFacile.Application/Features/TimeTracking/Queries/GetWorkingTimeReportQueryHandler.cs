// <copyright file="GetWorkingTimeReportQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.TimeTracking;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Handler for <see cref="GetWorkingTimeReportQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetWorkingTimeReportQueryHandler"/> class.
/// </remarks>
/// <param name="calculator">The working time calculator.</param>
/// <param name="access">The working time access rules.</param>
public sealed class GetWorkingTimeReportQueryHandler(WorkingTimeCalculator calculator, TimeAccess access) : IRequestHandler<GetWorkingTimeReportQuery, WorkingTimeReport>
{
    /// <inheritdoc/>
    public Task<WorkingTimeReport> Handle(GetWorkingTimeReportQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.To < request.From
            ? throw new ArgumentException("Il periodo non è valido.", nameof(request))
            : HandleInternalAsync(request, cancellationToken);
    }

    private async Task<WorkingTimeReport> HandleInternalAsync(GetWorkingTimeReportQuery request, CancellationToken cancellationToken)
    {
        await access.EnsureCanReadAsync(request.EmployeeId, cancellationToken).ConfigureAwait(false);

        return await calculator.EvaluateAsync(request.EmployeeId, request.From, request.To, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Dipendente non trovato.");
    }
}
