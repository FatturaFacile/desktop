namespace FatturaFacile.Infrastructure.Common.Configuration;

/// <summary>
/// Supported relational database providers for the FatturaFacile ERP engine.
/// </summary>
public enum DatabaseProvider
{
    Sqlite,
    PostgreSQL,
    SqlServer
}

/// <summary>
/// Localization configuration settings.
/// </summary>
public sealed class LocalizationOptions
{
    public string DefaultCulture { get; set; } = "en";
    public List<string> SupportedCultures { get; set; } = ["en", "it", "de", "fr"];
}

/// <summary>
/// Root configuration options for the FatturaFacile ERP engine.
/// </summary>
public sealed class FatturaFacileOptions
{
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Sqlite;
    public string ConnectionString { get; set; } = "Data Source=fattura-facile.sqlite";
    public bool EnableAuditTrail { get; set; } = true;
    public bool EnableTransactionalOutbox { get; set; } = true;
    public bool EnableOpenTelemetry { get; set; } = true;

    public LocalizationOptions Localization { get; } = new();

    /// <summary>
    /// Configures the persistence database provider and connection string.
    /// </summary>
    public FatturaFacileOptions UseDatabase(DatabaseProvider provider, string connectionString)
    {
        Provider = provider;
        ConnectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Configures multi-language localization settings.
    /// </summary>
    public FatturaFacileOptions ConfigureLocalization(IEnumerable<string>? supportedCultures = null, string defaultCulture = "en")
    {
        if (supportedCultures != null)
        {
            Localization.SupportedCultures = supportedCultures.ToList();
        }
        Localization.DefaultCulture = defaultCulture;
        return this;
    }
}
