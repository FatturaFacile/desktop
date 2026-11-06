// <copyright file="FolderSdiTransmitter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Invoicing.Sdi;
using FatturaFacile.Infrastructure.Persistence;

namespace FatturaFacile.Infrastructure.Services;

/// <summary>
/// SDI channel based on folders: the invoices are written to SDI\Uscita (to be sent with the web portal of the
/// Agenzia delle Entrate, by PEC or by an intermediary that synchronises the folder) and the notifications and supplier
/// invoices are read from SDI\Entrata; processed files are moved to SDI\Elaborati.
/// </summary>
/// <param name="paths">The data folder.</param>
public sealed class FolderSdiTransmitter(DatabasePaths paths) : ISdiTransmitter
{
    /// <inheritdoc />
    public string Channel => "Cartella " + Outbox;

    private string Outbox => Path.Combine(paths.DataDirectory, "SDI", "Uscita");

    private string Inbox => Path.Combine(paths.DataDirectory, "SDI", "Entrata");

    private string Processed => Path.Combine(paths.DataDirectory, "SDI", "Elaborati");

    /// <inheritdoc />
    public async Task<string?> SendAsync(string fileName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
    {
        _ = Directory.CreateDirectory(Outbox);
        await File.WriteAllBytesAsync(Path.Combine(Outbox, Path.GetFileName(fileName)), content, cancellationToken).ConfigureAwait(false);
        return null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SdiInboundFile>> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        _ = Directory.CreateDirectory(Inbox);
        List<SdiInboundFile> files = [];

        foreach (var path in Directory.EnumerateFiles(Inbox).Order(StringComparer.OrdinalIgnoreCase))
        {
            if (path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".p7m", StringComparison.OrdinalIgnoreCase))
            {
                files.Add(new SdiInboundFile(Path.GetFileName(path), await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false)));
            }
        }

        return files;
    }

    /// <inheritdoc />
    public Task AcknowledgeAsync(SdiInboundFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        _ = Directory.CreateDirectory(Processed);
        File.Move(Path.Combine(Inbox, file.FileName), Path.Combine(Processed, file.FileName), overwrite: true);
        return Task.CompletedTask;
    }
}
