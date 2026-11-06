// <copyright file="DeleteTimeEventCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Handler for <see cref="DeleteTimeEventCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DeleteTimeEventCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="access">The working time access rules.</param>
public sealed class DeleteTimeEventCommandHandler(IUnitOfWork unitOfWork, TimeAccess access) : IRequestHandler<DeleteTimeEventCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(DeleteTimeEventCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(DeleteTimeEventCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<TimeClockEvent>();
        var clock = await repository.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);

        if (clock is null || clock.DeletedAt is not null)
        {
            return false;
        }

        clock.DeletedAt = DateTime.UtcNow;
        clock.DeletionReason = request.Reason;
        repository.Update(clock);
        await access.AuditAsync(
            new TimeAuditEntry { EmployeeId = clock.EmployeeId, EntityType = nameof(TimeClockEvent), EntityId = clock.Id, Action = "Deleted", Before = TimeAuditFormat.Describe(clock), Reason = request.Reason },
            cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
