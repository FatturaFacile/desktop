// <copyright file="SendInvoiceToSdiCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Invoicing.Sdi;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Sdi;

/// <summary>
/// Handles <see cref="SendInvoiceToSdiCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="transmitter">The SDI channel.</param>
public sealed class SendInvoiceToSdiCommandHandler(IUnitOfWork unitOfWork, ISdiTransmitter transmitter) : IRequestHandler<SendInvoiceToSdiCommand, string>
{
    /// <inheritdoc />
    public Task<string> Handle(SendInvoiceToSdiCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.InvoiceId, cancellationToken);
    }

    /// <summary>
    /// Gives a new progressive to the file name of an invoice rejected by the SDI (a rejected file name cannot be reused).
    /// </summary>
    /// <param name="fileName">The previous file name.</param>
    /// <returns>The new file name.</returns>
    private static string NextFileName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var separator = name.AsSpan().LastIndexOf('_');
        var prefix = separator > 0 ? name[..separator] : name;
        var progressive = (DateTime.UtcNow.Ticks % 60_466_176).ToString("x5", CultureInfo.InvariantCulture).ToUpperInvariant();
        return $"{prefix}_{progressive[^5..]}{Path.GetExtension(fileName)}";
    }

    [SuppressMessage("Security", "Csharp_injection_rule:CommandInjection", Justification = "File name contains only safe alphanumeric progressive.")]
    private async Task<string> HandleInternalAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException("Fattura non trovata.");

        if (string.Equals(invoice.Status, "draft", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Emettere la fattura prima di inviarla allo SDI.");
        }

        var repository = unitOfWork.Repository<InvoiceSdiMetadata>();
        var metadata = (await repository.FindAsync(s => s.InvoiceId == invoiceId, cancellationToken).ConfigureAwait(false)).FirstOrDefault();

        if (metadata is null || string.IsNullOrWhiteSpace(metadata.XmlContent) || string.IsNullOrWhiteSpace(metadata.FileName))
        {
            throw new InvalidOperationException("La fattura non ha un file FatturaPA: l'invio allo SDI riguarda solo le fatture italiane.");
        }

        if (!SdiStatus.CanSend(metadata.SdiStatus))
        {
            throw new InvalidOperationException($"La fattura è già stata trasmessa (stato: {metadata.SdiStatus}).");
        }

        if (string.Equals(metadata.SdiStatus, SdiStatus.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            metadata.FileName = NextFileName(metadata.FileName);
        }

        var identifier = await transmitter.SendAsync(metadata.FileName, Encoding.UTF8.GetBytes(metadata.XmlContent), cancellationToken).ConfigureAwait(false);
        metadata.SdiStatus = SdiStatus.Sent;
        metadata.SentAt = DateTime.UtcNow;
        metadata.SdiIdentifier = identifier ?? metadata.SdiIdentifier;
        repository.Update(metadata);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return transmitter.Channel;
    }
}
