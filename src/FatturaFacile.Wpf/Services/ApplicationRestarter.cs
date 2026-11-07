// <copyright file="ApplicationRestarter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics;

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// WPF implementation of <see cref="IApplicationRestarter"/>.
/// </summary>
internal sealed class ApplicationRestarter : IApplicationRestarter
{
    /// <inheritdoc/>
    public void Restart()
    {
        var executable = Environment.ProcessPath;

        if (!string.IsNullOrEmpty(executable))
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = executable,
                UseShellExecute = false,
            };

            using var process = Process.Start(startInfo);
        }

        System.Windows.Application.Current.Shutdown();
    }
}
