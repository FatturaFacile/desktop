// <copyright file="FatturaFacileOptions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Infrastructure.Common.Configuration;

/// <summary>
/// Root configuration options for the FatturaFacile ERP engine.
/// </summary>
public sealed class FatturaFacileOptions
{
    /// <summary>
    /// Gets or sets the active database provider engine.
    /// </summary>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;

    /// <summary>
    /// Gets or sets the database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "Data Source=fattura-facile.sqlite";

    /// <summary>
    /// Gets or sets a value indicating whether entity audit trail logging is enabled.
    /// </summary>
    public bool EnableAuditTrail { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether transactional outbox domain event dispatching is enabled.
    /// </summary>
    public bool EnableTransactionalOutbox { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether OpenTelemetry metrics and distributed tracing are enabled.
    /// </summary>
    public bool EnableOpenTelemetry { get; set; } = true;

    /// <summary>
    /// Gets the localization configuration options.
    /// </summary>
    public LocalizationOptions Localization { get; } = new();

    /// <summary>
    /// Configures the persistence database provider and connection string.
    /// </summary>
    /// <param name="provider">The database provider to use.</param>
    /// <param name="connectionString">The ADO.NET connection string.</param>
    /// <returns>The updated <see cref="FatturaFacileOptions"/> instance for fluent chaining.</returns>
    public FatturaFacileOptions UseDatabase(DatabaseProvider provider, string connectionString)
    {
        Provider = provider;
        ConnectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Configures multi-language localization settings.
    /// </summary>
    /// <param name="supportedCultures">The optional collection of supported culture codes.</param>
    /// <param name="defaultCulture">The default fallback culture code (defaults to "en").</param>
    /// <returns>The updated <see cref="FatturaFacileOptions"/> instance for fluent chaining.</returns>
    public FatturaFacileOptions ConfigureLocalization(
        IEnumerable<string>? supportedCultures = null,
        string               defaultCulture    = "en")
    {
        if (supportedCultures != null)
        {
            Localization.SupportedCultures = [..supportedCultures];
        }

        Localization.DefaultCulture = defaultCulture;
        return this;
    }
}
