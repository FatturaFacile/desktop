// <copyright file="GetClockStatusQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.TimeTracking.Commands;
using FatturaFacile.Application.TimeTracking;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Queries;

/// <summary>
/// Handler for <see cref="GetClockStatusQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetClockStatusQueryHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="access">The working time access rules.</param>
public sealed class GetClockStatusQueryHandler(IUnitOfWork unitOfWork, TimeAccess access) : IRequestHandler<GetClockStatusQuery, ClockStatusDto>
{
    /// <inheritdoc/>
    public Task<ClockStatusDto> Handle(GetClockStatusQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(cancellationToken);
    }

    private async Task<ClockStatusDto> HandleInternalAsync(CancellationToken cancellationToken)
    {
        var employee = await access.CurrentEmployeeAsync(cancellationToken).ConfigureAwait(false);

        return employee is null
            ? new ClockStatusDto(Guid.Empty, string.Empty, "NotLinked", LastEvent: null, 0)
            : await ClockStatusReader.ReadAsync(unitOfWork, employee, DateTime.Now, cancellationToken).ConfigureAwait(false);
    }
}
