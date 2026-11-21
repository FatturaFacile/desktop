// <copyright file="JsonSecuritySettingsStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using System.Text.Json;
using FatturaFacile.Application.Security;

namespace FatturaFacile.Infrastructure.Security;

/// <summary>
/// Stores the security settings in a small JSON file of the data folder.
/// </summary>
/// <param name="path">The settings file path.</param>
public sealed class JsonSecuritySettingsStore(string path) : ISecuritySettingsStore
{
    private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    /// <inheritdoc/>
    public SecuritySettings Load()
    {
        try
        {
            return File.Exists(path)
                ? (JsonSerializer.Deserialize<SecuritySettings>(File.ReadAllText(path), _options) ?? new SecuritySettings()).Normalize()
                : new SecuritySettings();
        }
        catch (JsonException)
        {
            return new SecuritySettings();
        }
        catch (IOException)
        {
            return new SecuritySettings();
        }
    }

    /// <inheritdoc/>
    public void Save(SecuritySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(settings.Normalize(), _options));
    }
}
