// <copyright file="ISdiTransmitter.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Invoicing.Sdi;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Channel towards the Sistema di Interscambio (through an accredited intermediary or a folder synchronised with one).
/// </summary>
public interface ISdiTransmitter
{
    /// <summary>
    /// Gets the name of the channel shown to the user.
    /// </summary>
    public string Channel { get; }

    /// <summary>
    /// Sends an invoice file.
    /// </summary>
    /// <param name="fileName">The FatturaPA file name.</param>
    /// <param name="content">The file content.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The identifier returned by the channel, when available.</returns>
    public Task<string?> SendAsync(string fileName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the files received from the SDI: notifications on sent invoices and supplier invoices.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The received files.</returns>
    public Task<IReadOnlyList<SdiInboundFile>> ReceiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a received file as processed.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    public Task AcknowledgeAsync(SdiInboundFile file, CancellationToken cancellationToken = default);
}
