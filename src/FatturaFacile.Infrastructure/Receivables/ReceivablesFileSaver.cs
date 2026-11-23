// <copyright file="ReceivablesFileSaver.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using FatturaFacile.Application.Receivables;

namespace FatturaFacile.Infrastructure.Receivables;

/// <summary>
/// <see cref="IReceivablesFileSaver"/> that writes the statement in `Documents/FatturaFacile/Statements` of the user. Only the file
/// name is used, so a hostile name cannot leave that folder.
/// </summary>
public sealed class ReceivablesFileSaver : IReceivablesFileSaver
{
    private const string FolderName = "FatturaFacile";
    private const string SubFolderName = "Statements";
    private const string FallbackName = "statement.pdf";

    /// <inheritdoc/>
    public Task<string> SaveAsync(ReceivablesFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file);

        return SaveCoreAsync(file, cancellationToken);
    }

    private static async Task<string> SaveCoreAsync(ReceivablesFile file, CancellationToken cancellationToken)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FolderName, SubFolderName);
        _ = Directory.CreateDirectory(folder);

        var name = Path.GetFileName(file.FileName);
        var path = Path.Combine(folder, string.IsNullOrWhiteSpace(name) ? FallbackName : name);
        await File.WriteAllBytesAsync(path, file.Content.ToArray(), cancellationToken).ConfigureAwait(false);

        return path;
    }
}
