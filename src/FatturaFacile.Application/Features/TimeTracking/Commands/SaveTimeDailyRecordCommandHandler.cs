// <copyright file="SaveTimeDailyRecordCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Handler for <see cref="SaveTimeDailyRecordCommand"/>: employees record their own days, managers any; changes are audited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SaveTimeDailyRecordCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="access">The working time access rules.</param>
public sealed class SaveTimeDailyRecordCommandHandler(IUnitOfWork unitOfWork, TimeAccess access) : IRequestHandler<SaveTimeDailyRecordCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(SaveTimeDailyRecordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Record, request.Reason, cancellationToken);
    }

    private async Task<Guid> HandleInternalAsync(TimeDailyRecord values, string? reason, CancellationToken cancellationToken)
    {
        if (!access.CanManage)
        {
            var own = await access.CurrentEmployeeAsync(cancellationToken).ConfigureAwait(false);

            if (own?.Id != values.EmployeeId)
            {
                throw new UnauthorizedAccessException("Si possono registrare solo le proprie ore.");
            }
        }

        var repository = unitOfWork.Repository<TimeDailyRecord>();
        var employeeId = values.EmployeeId;
        var day = values.Date.Date;
        var existing = (await repository.FindAsync(r => r.EmployeeId == employeeId && r.Date == day, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (existing is null)
        {
            TimeDailyRecord created = new()
            {
                EmployeeId = employeeId,
                Date = day,
                WorkedMinutes = values.WorkedMinutes,
                StartTime = values.StartTime,
                EndTime = values.EndTime,
                Note = values.Note,
                CreatedBy = access.UserName,
            };

            await repository.AddAsync(created, cancellationToken).ConfigureAwait(false);
            await access.AuditAsync(
                new TimeAuditEntry { EmployeeId = employeeId, EntityType = nameof(TimeDailyRecord), EntityId = created.Id, Action = "Created", After = TimeAuditFormat.Describe(created), Reason = reason },
                cancellationToken).ConfigureAwait(false);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return created.Id;
        }

        var before = TimeAuditFormat.Describe(existing);
        existing.WorkedMinutes = values.WorkedMinutes;
        existing.StartTime = values.StartTime;
        existing.EndTime = values.EndTime;
        existing.Note = values.Note;
        existing.UpdatedAt = DateTime.UtcNow;
        repository.Update(existing);
        await access.AuditAsync(
            new TimeAuditEntry { EmployeeId = employeeId, EntityType = nameof(TimeDailyRecord), EntityId = existing.Id, Action = "Modified", Before = before, After = TimeAuditFormat.Describe(existing), Reason = reason },
            cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return existing.Id;
    }
}
