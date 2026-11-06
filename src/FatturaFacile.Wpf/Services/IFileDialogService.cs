// <copyright file="IFileDialogService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// Abstraction over the Windows open/save file dialogs (keeps view models testable).
/// </summary>
internal interface IFileDialogService
{
    /// <summary>
    /// Shows a save file dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="filter">The file filter.</param>
    /// <param name="defaultFileName">The proposed file name.</param>
    /// <returns>The chosen path, or null when cancelled.</returns>
    public string? ShowSaveFile(string title, string filter, string defaultFileName);

    /// <summary>
    /// Shows an open file dialog.
    /// </summary>
    /// <param name="title">The dialog title.</param>
    /// <param name="filter">The file filter.</param>
    /// <returns>The chosen path, or null when cancelled.</returns>
    public string? ShowOpenFile(string title, string filter);
}
