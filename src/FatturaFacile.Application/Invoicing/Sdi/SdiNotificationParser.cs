// <copyright file="SdiNotificationParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;

namespace FatturaFacile.Application.Invoicing.Sdi;

/// <summary>
/// Reads the notifications of the Sistema di Interscambio (schema MessaggiTypes v1.0).
/// </summary>
public static class SdiNotificationParser
{
    private static readonly string[] Codes = ["RC", "NS", "MC", "NE", "DT", "AT", "EC"];

    /// <summary>
    /// Gets a value indicating whether a file name is the one of an SDI notification (for example IT01234567890_00001_RC_001.xml).
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns><see langword="true"/> for a notification.</returns>
    public static bool IsNotificationFile(string fileName)
    {
        var parts = Path.GetFileNameWithoutExtension(fileName).Split('_');
        return parts.Length >= 4 && Codes.Contains(parts[2].ToUpperInvariant());
    }

    /// <summary>
    /// Gets a value indicating whether a file name is the metadata file sent with a received invoice.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns><see langword="true"/> for a metadata file.</returns>
    public static bool IsMetadataFile(string fileName) =>
        Path.GetFileNameWithoutExtension(fileName).Contains("_MT_", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Reads a notification.
    /// </summary>
    /// <param name="content">The XML file.</param>
    /// <returns>The notification, or <see langword="null"/> when the document is not an SDI notification.</returns>
    public static SdiNotification? Parse(ReadOnlyMemory<byte> content)
    {
        using var stream = new MemoryStream(content.ToArray());
        var root = XDocument.Load(stream).Root;

        if (root is null || TypeOf(root.Name.LocalName) is not { } type)
        {
            return null;
        }

        SdiNotification notification = new()
        {
            Type = type,
            SdiIdentifier = Value(root, "IdentificativoSdI"),
            InvoiceFileName = Value(root, "NomeFile"),
            Outcome = Value(root, "Esito"),
            Timestamp = ParseDate(Value(root, "DataOraConsegna") ?? Value(root, "DataOraRicezione")),
        };

        foreach (var error in root.Descendants().Where(static e => string.Equals(e.Name.LocalName, "Errore", StringComparison.Ordinal)))
        {
            string?[] parts = [Value(error, "Codice"), Value(error, "Descrizione"), Value(error, "Suggerimento")];
            notification.Errors.Add(string.Join(' ', parts.Where(static s => !string.IsNullOrWhiteSpace(s))));
        }

        return notification;
    }

    private static string? TypeOf(string localName) => localName switch
    {
        "RicevutaConsegna" => "RC",
        "NotificaScarto" => "NS",
        "NotificaMancataConsegna" => "MC",
        "NotificaEsito" => "NE",
        "NotificaDecorrenzaTermini" => "DT",
        "AttestazioneTrasmissioneFattura" => "AT",
        _ => null,
    };

    private static string? Value(XElement parent, string localName) =>
        parent.Descendants().FirstOrDefault(e => string.Equals(e.Name.LocalName, localName, StringComparison.Ordinal))?.Value.Trim();

    private static DateTime? ParseDate(string? text) =>
        DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var value) ? value.LocalDateTime : null;
}
