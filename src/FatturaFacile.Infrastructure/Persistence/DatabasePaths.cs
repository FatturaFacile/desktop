// <copyright file="DatabasePaths.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// Absolute locations of the SQLite database and of the field encryption key file.
/// </summary>
/// <param name="DatabasePath">The database file path.</param>
/// <param name="KeyPath">The DPAPI-protected key file path.</param>
public sealed record DatabasePaths(string DatabasePath, string KeyPath)
{
    /// <summary>
    /// Gets the data folder containing the database, the report templates and the document archive.
    /// </summary>
    public string DataDirectory => Path.GetDirectoryName(Path.GetFullPath(DatabasePath)) ?? ".";
}
