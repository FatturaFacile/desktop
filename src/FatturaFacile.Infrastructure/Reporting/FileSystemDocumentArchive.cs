// <copyright file="FileSystemDocumentArchive.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Infrastructure.Persistence;

namespace FatturaFacile.Infrastructure.Reporting;

/// <summary>
/// Saves generated documents in "Documents\&lt;company&gt;\&lt;type&gt;" inside the data folder.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FileSystemDocumentArchive"/> class.
/// </remarks>
/// <param name="paths">The data paths.</param>
public sealed class FileSystemDocumentArchive(DatabasePaths paths) : IDocumentArchive
{
    /// <inheritdoc/>
    public Task<string> SaveAsync(string companyName, ReportDocumentType documentType, string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);

        var directory = Path.Combine(paths.DataDirectory, "Documents", Safe(companyName), documentType.ToString());
        _ = Directory.CreateDirectory(directory);
        var target = Path.Combine(directory, Safe(Path.GetFileName(fileName)));

        return SaveInternalAsync(target, content, cancellationToken);
    }

    private static string Safe(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string([.. value.Select(c => invalid.Contains(c) ? '_' : c)]).Trim();
        return safe.Length == 0 ? "_" : safe;
    }

    private static async Task<string> SaveInternalAsync(string target, byte[] content, CancellationToken cancellationToken)
    {
        await File.WriteAllBytesAsync(target, content, cancellationToken).ConfigureAwait(false);
        return target;
    }
}
