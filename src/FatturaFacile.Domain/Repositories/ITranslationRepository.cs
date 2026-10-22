// <copyright file="ITranslationRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Exposes specialized localization persistence and query operations for <see cref="AppTranslation"/> entities across multilingual tenant environments.
/// </summary>
public interface ITranslationRepository : IRepository<AppTranslation, Guid>
{
    /// <summary>
    /// Asynchronously retrieves all localized string entries for a specific culture locale, considering tenant-specific customizations and system fallbacks.
    /// </summary>
    /// <param name="locale">The target ISO culture code or locale tag (e.g., 'it-CH', 'de-CH', 'en-US').</param>
    /// <param name="tenantId">The optional tenant identifier for custom translation overrides, or <see langword="null"/> to query system-wide base translations.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous query; contains a read-only list of <see cref="AppTranslation"/> records matching the locale.</returns>
    public Task<IReadOnlyList<AppTranslation>> GetByLocaleAsync(
        string locale,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an exact localized translation entry for a specific localization key and culture locale.
    /// </summary>
    /// <param name="locale">The target ISO culture code or locale tag (e.g., 'it-CH', 'de-CH', 'en-US').</param>
    /// <param name="key">The unique hierarchical localization resource key (e.g., 'Invoices.Status.Pending').</param>
    /// <param name="tenantId">The optional tenant identifier for tenant-specific overrides, or <see langword="null"/> to look up default system localization.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during the asynchronous query.</param>
    /// <returns>A task representing the asynchronous lookup; contains the matching <see cref="AppTranslation"/> if defined, or <see langword="null"/> otherwise.</returns>
    public Task<AppTranslation?> GetByKeyAsync(
        string locale,
        string key,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);
}
