// <copyright file="SyncOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// Configuration of the web-api synchronization (section "Sync").
/// </summary>
public sealed class SyncOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Sync";

    /// <summary>
    /// The separator that terminates the API root URL.
    /// </summary>
    public const char UrlSeparator = (char)47;

    /// <summary>
    /// Gets or sets the web-api root URL (for example "https://api.example.com/"). Empty disables synchronization.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the seconds between automatic sync passes.
    /// </summary>
    public int IntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Gets a value indicating whether synchronization is configured.
    /// </summary>
    public bool IsEnabled => !string.IsNullOrWhiteSpace(BaseUrl);
}
