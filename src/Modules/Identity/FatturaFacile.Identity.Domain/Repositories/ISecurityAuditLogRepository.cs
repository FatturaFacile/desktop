// <copyright file="ISecurityAuditLogRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Shared.Repositories;
using FatturaFacile.Identity.Domain.Entities;

namespace FatturaFacile.Identity.Domain.Repositories;

/// <summary>
/// Domain repository contract for appending and querying security audit log events.
/// </summary>
public interface ISecurityAuditLogRepository : IRepository
{
    /// <summary>
    /// Appends a new security audit log entry to the persistent store.
    /// </summary>
    /// <param name="log">The security audit log instance.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task AddAsync(SecurityAuditLog log, CancellationToken cancellationToken = default);
}
