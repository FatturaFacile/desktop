// <copyright file="IShellLauncher.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Services;

/// <summary>
/// Opens files and folders with the program associated by Windows (Word for templates, Explorer for folders).
/// </summary>
internal interface IShellLauncher
{
    /// <summary>
    /// Opens a file or folder.
    /// </summary>
    /// <param name="path">The path.</param>
    public void Open(string path);
}
