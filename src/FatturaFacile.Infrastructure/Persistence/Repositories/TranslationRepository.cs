// <copyright file="TranslationRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Repositories;

using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository managing user-customized and system default UI translations persisted in the SQLite store.
/// </summary>
/// <param name="dbContext">The application database context.</param>
public sealed class TranslationRepository(FatturaFacileDbContext dbContext)
    : Repository<AppTranslation, Guid>(dbContext: dbContext), ITranslationRepository
{

    /// <summary>
    /// Retrieves all translations registered for a specific ISO locale and optional tenant scope.
    /// </summary>
    /// <param name="locale">The ISO language code (e.g., "it", "de", "fr", "en").</param>
    /// <param name="tenantId">The optional tenant identifier for tenant-specific overrides, or <see langword="null"/> for global entries.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only collection of translation entities matching the criteria.</returns>
    public async Task<IReadOnlyList<AppTranslation>> GetByLocaleAsync(
        string            locale,
        Guid?             tenantId          = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
                    .Where(predicate: t => t.Locale == locale && t.TenantId == tenantId)
                    .ToListAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Retrieves a specific translation entry matching the given locale, key, and optional tenant scope.
    /// </summary>
    /// <param name="locale">The ISO language code.</param>
    /// <param name="key">The translation identifier key.</param>
    /// <param name="tenantId">The optional tenant identifier, or <see langword="null"/> for global entries.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="AppTranslation"/> if found; otherwise, <see langword="null"/>.</returns>
    public Task<AppTranslation?> GetByKeyAsync(
        string            locale,
        string            key,
        Guid?             tenantId          = null,
        CancellationToken cancellationToken = default)
    {
        return DbSet
           .FirstOrDefaultAsync(
                                predicate: t => t.Locale == locale && t.Key == key && t.TenantId == tenantId,
                                cancellationToken: cancellationToken);
    }

}
