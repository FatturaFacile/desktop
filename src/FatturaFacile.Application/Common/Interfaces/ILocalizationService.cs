// <copyright file="ILocalizationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Provides application-wide localization and translation capabilities supporting multi-lingual UI and tenant overrides.
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
    /// Gets the current active culture locale code (e.g., "it", "de", "en", "fr").
    /// </summary>
    public string CurrentLocale { get; }

    /// <summary>
    /// Switches the active application culture locale and notifies subscribers.
    /// </summary>
    /// <param name="locale">The two-letter ISO language name or culture code to activate.</param>
    public void SetLocale(
        string locale);

    /// <summary>
    /// Resolves the localized string for a specified resource key synchronously, falling back to a default value if missing.
    /// </summary>
    /// <param name="key">The unique resource identifier key.</param>
    /// <param name="defaultFallback">An optional fallback string to return when the resource key is not resolved.</param>
    /// <returns>The resolved localized string value, or <paramref name="defaultFallback"/> if not found.</returns>
    public string GetString(
        string key,
        string? defaultFallback = null);

    /// <summary>
    /// Asynchronously retrieves the localized string for a key under a specified or active locale.
    /// </summary>
    /// <param name="key">The unique resource identifier key.</param>
    /// <param name="locale">The optional locale override; if omitted or <see langword="null"/>, uses <see cref="CurrentLocale"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous lookup returning the resolved localized text.</returns>
    public Task<string> GetStringAsync(
        string key,
        string? locale = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all default and custom translated text entries for a target locale.
    /// </summary>
    /// <param name="locale">The culture locale code whose translation catalogue is requested.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous retrieval of translation transfer objects.</returns>
    public Task<IReadOnlyList<AppTranslationDto>> GetAllTranslationsAsync(
        string locale,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists or updates a customized translation text override for a specific locale and key.
    /// </summary>
    /// <param name="locale">The target culture locale code.</param>
    /// <param name="key">The translation identifier key.</param>
    /// <param name="customValue">The tenant-specific custom text value to persist.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous storage operation.</returns>
    public Task SaveCustomTranslationAsync(
        string locale,
        string key,
        string customValue,
        CancellationToken cancellationToken = default);
}
