// <copyright file="ShellLauncher.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.IO;

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

        var explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
        ProcessStartInfo startInfo = new()
        {
            FileName = explorerPath,
            UseShellExecute = false,
        };
        startInfo.ArgumentList.Add(path);

        using var process = Process.Start(startInfo);
    }
}
