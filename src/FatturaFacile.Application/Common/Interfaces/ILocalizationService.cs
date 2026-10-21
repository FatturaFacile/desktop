// <copyright file="ILocalizationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Defines the contract for localization service.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Occurs when the application's current language changes.
    /// </summary>
    /// <remarks>
    /// Subscribers can listen to this event to refresh localized content,
    /// update user interface elements, and reload translated resources.
    /// </remarks>
    public event EventHandler? LanguageChanged;

    /// <summary>
    /// Gets the current locale.
    /// </summary>
    public string CurrentLocale { get; }

    /// <summary>
    /// Set Locale.
    /// </summary>
    /// <param name="locale">The locale.</param>
    public void SetLocale(
        string locale);

    /// <summary>
    /// Get String.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="defaultFallback">The default fallback.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public string GetString(
        string key,
        string? defaultFallback = null);

    /// <summary>
    /// Get String Async.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="locale">The locale.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<string> GetStringAsync(
        string key,
        string? locale = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get All Translations Async.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<IReadOnlyList<AppTranslationDto>> GetAllTranslationsAsync(
        string locale,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Save Custom Translation Async.
    /// </summary>
    /// <param name="locale">The locale.</param>
    /// <param name="key">The key.</param>
    /// <param name="customValue">The custom value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task SaveCustomTranslationAsync(
        string locale,
        string key,
        string customValue,
        CancellationToken cancellationToken = default);
}
