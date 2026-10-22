// <copyright file="TranslationRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Represents translation repository.
/// </summary>
/// <param name="dbContext">The db context.</param>
public sealed class TranslationRepository(FatturaFacileDbContext dbContext)
    : Repository<AppTranslation, Guid>(dbContext), ITranslationRepository
{
    /// <summary>
    /// Get By Locale Async.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public async Task<IReadOnlyList<AppTranslation>> GetByLocaleAsync(
        string locale,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.Locale == locale && t.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get By Key Async.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <param name="key">The key.</param>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<AppTranslation?> GetByKeyAsync(
        string locale,
        string key,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .FirstOrDefaultAsync(
                t => t.Locale == locale && t.Key == key && t.TenantId == tenantId,
                cancellationToken);
    }
}
