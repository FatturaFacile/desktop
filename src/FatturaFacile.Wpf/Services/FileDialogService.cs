// <copyright file="FileDialogService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Win32;

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// Windows implementation of <see cref="IFileDialogService"/>.
/// </summary>
internal sealed class FileDialogService : IFileDialogService
{
    /// <inheritdoc/>
    public string? ShowSaveFile(string title, string filter, string defaultFileName)
    {
        SaveFileDialog dialog = new()
        {
            Title = title,
            Filter = filter,
            FileName = defaultFileName,
            OverwritePrompt = true,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    /// <inheritdoc/>
    public string? ShowOpenFile(string title, string filter)
    {
        OpenFileDialog dialog = new()
        {
            Title = title,
            Filter = filter,
            CheckFileExists = true,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
