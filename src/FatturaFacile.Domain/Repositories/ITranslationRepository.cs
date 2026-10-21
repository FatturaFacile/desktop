// <copyright file="ITranslationRepository.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Administration;

namespace FatturaFacile.Domain.Repositories;

/// <summary>
/// Defines the contract for translation repository.
/// </summary>
public interface ITranslationRepository : IRepository<AppTranslation, Guid>
{
    /// <summary>
    /// Get By Locale Async.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<AppTranslation>> GetByLocaleAsync(
        string locale,
        Guid? tenantId = null,
        CancellationToken cancellationToken = default);

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
        CancellationToken cancellationToken = default);
}
