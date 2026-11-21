// <copyright file="ISecuritySettingsStore.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Persists the security settings of this device.
/// </summary>
public interface ISecuritySettingsStore
{
    /// <summary>
    /// Loads the settings (defaults when none were saved or the file is unreadable).
    /// </summary>
    /// <returns>The settings.</returns>
    public SecuritySettings Load();

    /// <summary>
    /// Saves the settings.
    /// </summary>
    /// <param name="settings">The settings.</param>
    public void Save(SecuritySettings settings);
}
