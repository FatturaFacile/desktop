// <copyright file="SecurityAuditLogRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Identity.Domain.Entities;
using FatturaFacile.Identity.Domain.Repositories;
using FatturaFacile.Identity.Infrastructure.Persistence;

namespace FatturaFacile.Identity.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the <see cref="ISecurityAuditLogRepository"/> interface.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SecurityAuditLogRepository"/> class.
/// </remarks>
/// <param name="context">The identity database context.</param>
public sealed class SecurityAuditLogRepository(IdentityDbContext context) : ISecurityAuditLogRepository
{
    private readonly IdentityDbContext _context = context;

    /// <inheritdoc />
    public async Task AddAsync(SecurityAuditLog log, CancellationToken cancellationToken = default)
    {
        _ = await _context.SecurityAuditLogs.AddAsync(log, cancellationToken);
        _ = await _context.SaveChangesAsync(cancellationToken);
    }
}
