// <copyright file="ShellLauncher.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics;

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// Opens files and folders through the Windows shell.
/// </summary>
internal sealed class ShellLauncher : IShellLauncher
{
    /// <inheritdoc/>
    public void Open(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var process = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
    }
}
