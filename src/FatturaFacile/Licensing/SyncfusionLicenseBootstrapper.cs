// <copyright file="SyncfusionLicenseBootstrapper.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.Configuration;
using Syncfusion.Licensing;

namespace FatturaFacile.Wpf.Licensing;

/// <summary>
/// Bootstraps Syncfusion UI controls licensing from appsettings configuration or environment variables.
/// </summary>
public static class SyncfusionLicenseBootstrapper
{
    /// <summary>
    /// Registers the Syncfusion license key if available.
    /// </summary>
    /// <param name="configuration">The application configuration root.</param>
    public static void RegisterLicense(IConfiguration configuration)
    {
        // 1. Try reading from appsettings.json
        var licenseKey = configuration["Syncfusion:LicenseKey"];

        // 2. Try environment variable fallback
        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            licenseKey = Environment.GetEnvironmentVariable("SYNCFUSION_LICENSE_KEY");
        }

        if (!string.IsNullOrWhiteSpace(licenseKey) && licenseKey != "YOUR_SYNCFUSION_COMMUNITY_LICENSE_KEY_HERE")
        {
            SyncfusionLicenseProvider.RegisterLicense(licenseKey.Trim());
        }
    }
}
