// <copyright file="ProcessSdiInboxCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Xml;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Invoicing.Sdi;
using FatturaFacile.Application.Purchasing;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Sdi;

/// <summary>
/// Handles <see cref="ProcessSdiInboxCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="transmitter">The SDI channel.</param>
/// <param name="importer">The supplier invoice importer.</param>
/// <param name="signedDocumentReader">Extracts the XML from signed P7M files.</param>
public sealed class ProcessSdiInboxCommandHandler(
    IUnitOfWork unitOfWork,
    ISdiTransmitter transmitter,
    PurchaseInvoiceImporter importer,
    ISignedDocumentReader signedDocumentReader) : IRequestHandler<ProcessSdiInboxCommand, SdiInboxResult>
{
    /// <inheritdoc />
    public async Task<SdiInboxResult> Handle(ProcessSdiInboxCommand request, CancellationToken cancellationToken)
    {
        var files = await transmitter.ReceiveAsync(cancellationToken).ConfigureAwait(false);
        var notifications = 0;
        var purchases = 0;
        List<string> messages = [];

        foreach (var file in files)
        {
            try
            {
                if (SdiNotificationParser.IsMetadataFile(file.FileName))
                {
                    await transmitter.AcknowledgeAsync(file, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (SdiNotificationParser.IsNotificationFile(file.FileName))
                {
                    var message = await ApplyNotificationAsync(file, cancellationToken).ConfigureAwait(false);
                    notifications++;

                    if (message is not null)
                    {
                        messages.Add(message);
                    }
                }
                else
                {
                    purchases += (await importer.ImportAsync(file.FileName, file.Content, cancellationToken).ConfigureAwait(false)).Created;
                }

                await transmitter.AcknowledgeAsync(file, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is InvalidDataException or XmlException or InvalidOperationException)
            {
                messages.Add($"{file.FileName}: {ex.Message}");
            }
        }

        return new SdiInboxResult(notifications, purchases, messages);
    }

    private static string BaseName(string? fileName)
    {
        var name = Path.GetFileName(fileName ?? string.Empty);
        return name.EndsWith(".p7m", StringComparison.OrdinalIgnoreCase) ? name[..^4] : name;
    }

    private async Task<string?> ApplyNotificationAsync(SdiInboundFile file, CancellationToken cancellationToken)
    {
        var notification = SdiNotificationParser.Parse(signedDocumentReader.Unwrap(file.Content))
            ?? throw new InvalidDataException("Notifica SDI non riconosciuta.");
        var target = BaseName(notification.InvoiceFileName);
        var repository = unitOfWork.Repository<InvoiceSdiMetadata>();
        var candidates = await repository.FindAsync(s => s.FileName != null, cancellationToken).ConfigureAwait(false);
        var metadata = candidates.FirstOrDefault(s => string.Equals(BaseName(s.FileName), target, StringComparison.OrdinalIgnoreCase));

        if (metadata is null)
        {
            return $"{file.FileName}: nessuna fattura con file {target}.";
        }

        metadata.SdiStatus = notification.ResultingStatus ?? metadata.SdiStatus;
        metadata.SdiIdentifier = notification.SdiIdentifier ?? metadata.SdiIdentifier;

        if (string.Equals(notification.Type, "RC", StringComparison.Ordinal) || string.Equals(notification.Type, "MC", StringComparison.Ordinal))
        {
            metadata.DeliveredAt = notification.Timestamp ?? DateTime.Now;
        }

        if (notification.Errors.Count > 0)
        {
            metadata.ValidationErrors = string.Join(Environment.NewLine, notification.Errors);
        }

        repository.Update(metadata);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return notification.Errors.Count > 0
            ? $"{target} scartata: {string.Join("; ", notification.Errors)}"
            : null;
    }
}
