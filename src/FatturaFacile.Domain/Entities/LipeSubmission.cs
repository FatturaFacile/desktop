// <copyright file="LipeSubmission.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// A LIPE communication prepared from a VAT return: the generated XML, its schema validation, the signature check and the transmission tracking.
/// </summary>
public class LipeSubmission : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the VAT return the communication reports.
    /// </summary>
    public Guid VatReturnId { get; set; }

    /// <summary>
    /// Gets or sets the tax year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the month of a monthly communication.
    /// </summary>
    public int? Month { get; set; }

    /// <summary>
    /// Gets or sets the quarter of a quarterly communication.
    /// </summary>
    public int? Quarter { get; set; }

    /// <summary>
    /// Gets or sets the name of the XML file.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated XML.
    /// </summary>
    public string XmlContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SHA-256 of the XML bytes, as lowercase hexadecimal.
    /// </summary>
    public string XmlSha256 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the XML passed the validation.
    /// </summary>
    public bool IsXsdValid { get; set; }

    /// <summary>
    /// Gets or sets the validation errors, one per line.
    /// </summary>
    public string? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets the lifecycle status.
    /// </summary>
    public LipeTransmissionStatus Status { get; set; } = LipeTransmissionStatus.Draft;

    /// <summary>
    /// Gets or sets the name of the signed file attached.
    /// </summary>
    public string? SignedFileName { get; set; }

    /// <summary>
    /// Gets or sets when the signed copy was verified and attached.
    /// </summary>
    public DateTime? SignedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets when the file was transmitted.
    /// </summary>
    public DateTime? TransmittedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the protocol of the receipt of the tax authority.
    /// </summary>
    public string? ReceiptProtocol { get; set; }

    /// <summary>
    /// Gets or sets the outcome text of the receipt.
    /// </summary>
    public string? ReceiptNote { get; set; }

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
