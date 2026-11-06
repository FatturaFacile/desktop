// <copyright file="SaveTimeEventCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Handler for <see cref="SaveTimeEventCommand"/>: every insertion and correction is written to the audit trail.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SaveTimeEventCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="access">The working time access rules.</param>
public sealed class SaveTimeEventCommandHandler(IUnitOfWork unitOfWork, TimeAccess access) : IRequestHandler<SaveTimeEventCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(SaveTimeEventCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Event, request.Reason, cancellationToken);
    }

    private async Task<Guid> HandleInternalAsync(TimeClockEvent values, string reason, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<TimeClockEvent>();
        var existing = values.Id == Guid.Empty ? null : await repository.GetByIdAsync(values.Id, cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            TimeClockEvent created = new()
            {
                EmployeeId = values.EmployeeId,
                Timestamp = values.Timestamp,
                EventType = values.EventType,
                Source = "Manual",
                Note = values.Note,
                CreatedBy = access.UserName,
            };

            await repository.AddAsync(created, cancellationToken).ConfigureAwait(false);
            await access.AuditAsync(
                new TimeAuditEntry { EmployeeId = created.EmployeeId, EntityType = nameof(TimeClockEvent), EntityId = created.Id, Action = "Created", After = TimeAuditFormat.Describe(created), Reason = reason },
                cancellationToken).ConfigureAwait(false);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return created.Id;
        }

        if (existing.DeletedAt is not null)
        {
            throw new InvalidOperationException("Una timbratura eliminata non può essere modificata: inserirne una nuova.");
        }

        var before = TimeAuditFormat.Describe(existing);
        existing.Timestamp = values.Timestamp;
        existing.EventType = values.EventType;
        existing.Note = values.Note;
        existing.Source = "Manual";
        existing.UpdatedAt = DateTime.UtcNow;
        repository.Update(existing);
        await access.AuditAsync(
            new TimeAuditEntry { EmployeeId = existing.EmployeeId, EntityType = nameof(TimeClockEvent), EntityId = existing.Id, Action = "Modified", Before = before, After = TimeAuditFormat.Describe(existing), Reason = reason },
            cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return existing.Id;
    }
}
