// <copyright file="ClockCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Handler for <see cref="ClockCommand"/>: the sequence entry → (break start → break end)* → exit is enforced.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ClockCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="access">The working time access rules.</param>
public sealed class ClockCommandHandler(IUnitOfWork unitOfWork, TimeAccess access) : IRequestHandler<ClockCommand, ClockStatusDto>
{
    /// <inheritdoc/>
    public Task<ClockStatusDto> Handle(ClockCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.EventType, cancellationToken);
    }

    private static bool IsAllowed(string state, string eventType)
    {
        static bool Is(string value, string expected) => string.Equals(value, expected, StringComparison.Ordinal);

        var allowedWhenOff = Is(state, ClockStatusReader.Off) && Is(eventType, TimeEventType.In);
        var allowedWhenWorking = Is(state, ClockStatusReader.Working) && (Is(eventType, TimeEventType.Out) || Is(eventType, TimeEventType.BreakStart));
        var allowedOnBreak = Is(state, ClockStatusReader.OnBreak) && (Is(eventType, TimeEventType.BreakEnd) || Is(eventType, TimeEventType.Out));
        return allowedWhenOff || allowedWhenWorking || allowedOnBreak;
    }

    private async Task<ClockStatusDto> HandleInternalAsync(string eventType, CancellationToken cancellationToken)
    {
        var employee = await access.CurrentEmployeeAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Il tuo utente non è collegato a un dipendente: chiedere all'amministratore di collegarlo nella scheda del dipendente.");
        var now = DateTime.Now;
        var status = await ClockStatusReader.ReadAsync(unitOfWork, employee, now, cancellationToken).ConfigureAwait(false);

        if (!IsAllowed(status.State, eventType))
        {
            throw new InvalidOperationException($"Timbratura non valida nello stato attuale ({status.State}).");
        }

        TimeClockEvent clock = new()
        {
            EmployeeId = employee.Id,
            Timestamp = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, DateTimeKind.Unspecified),
            EventType = eventType,
            Source = "Terminal",
            CreatedBy = access.UserName,
        };

        await unitOfWork.Repository<TimeClockEvent>().AddAsync(clock, cancellationToken).ConfigureAwait(false);
        await access.AuditAsync(
            new TimeAuditEntry { EmployeeId = employee.Id, EntityType = nameof(TimeClockEvent), EntityId = clock.Id, Action = "Created", After = TimeAuditFormat.Describe(clock) },
            cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return await ClockStatusReader.ReadAsync(unitOfWork, employee, now, cancellationToken).ConfigureAwait(false);
    }
}
