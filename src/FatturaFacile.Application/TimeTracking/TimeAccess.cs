// <copyright file="TimeAccess.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.TimeTracking;

/// <summary>
/// Access rules of the working time data (personal data, LPD): managers see every employee, the other users only the
/// employee linked to their account.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TimeAccess"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentUserService">The current user.</param>
/// <param name="currentTenantService">The current company.</param>
public sealed class TimeAccess(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ICurrentTenantService currentTenantService)
{
    /// <summary>
    /// Gets the name of the current user, for the audit trail.
    /// </summary>
    public string UserName => currentUserService.User?.UserName ?? "?";

    /// <summary>
    /// Gets a value indicating whether the current user can read the working time of every employee.
    /// </summary>
    public bool CanReadAll => currentUserService.HasPermission(currentTenantService.TenantId, Permission.TimeRead);

    /// <summary>
    /// Gets a value indicating whether the current user can correct working time records.
    /// </summary>
    public bool CanManage => currentUserService.HasPermission(currentTenantService.TenantId, Permission.TimeManage);

    /// <summary>
    /// Returns the employee linked to the current user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The employee, or null.</returns>
    public async Task<HrEmployee?> CurrentEmployeeAsync(CancellationToken cancellationToken = default)
    {
        if (currentUserService.User is not { } user)
        {
            return null;
        }

        var userId = user.UserId;
        var employees = await unitOfWork.Repository<HrEmployee>().FindAsync(e => e.UserId == userId, cancellationToken).ConfigureAwait(false);
        return employees.FirstOrDefault();
    }

    /// <summary>
    /// Ensures that the current user can read the data of an employee.
    /// </summary>
    /// <param name="employeeId">The employee.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the check.</returns>
    /// <exception cref="UnauthorizedAccessException">When the employee is not the user's own.</exception>
    public async Task EnsureCanReadAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        if (CanReadAll)
        {
            return;
        }

        var own = await CurrentEmployeeAsync(cancellationToken).ConfigureAwait(false);

        if (own?.Id != employeeId)
        {
            throw new UnauthorizedAccessException("Si possono consultare solo le proprie timbrature.");
        }
    }

    /// <summary>
    /// Writes an audit entry (not saved: the caller saves the unit of work).
    /// </summary>
    /// <param name="entry">The audit entry without user and time.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    public Task AuditAsync(TimeAuditEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        entry.UserName = UserName;
        entry.Timestamp = DateTime.Now;
        return unitOfWork.Repository<TimeAuditEntry>().AddAsync(entry, cancellationToken);
    }
}
