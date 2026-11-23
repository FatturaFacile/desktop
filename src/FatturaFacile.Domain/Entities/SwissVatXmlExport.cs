// <copyright file="SwissVatXmlExport.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Persisted audit record of a SuisseTax eCH-0217 XML export and XSD validation result.
/// </summary>
public class SwissVatXmlExport : ITenantScopedEntity
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
    /// Gets or sets the associated Swiss VAT return identifier.
    /// </summary>
    public Guid SwissVatReturnId { get; set; }

    /// <summary>
    /// Gets or sets the exported file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SHA-256 checksum of the XML payload.
    /// </summary>
    public string Sha256Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the XML passed eCH-0217 XSD validation.
    /// </summary>
    public bool XsdValid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether quadratura passed.
    /// </summary>
    public bool QuadraturaPassed { get; set; }

    /// <summary>
    /// Gets or sets the serialized XML payload.
    /// </summary>
    public string XmlPayload { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC export timestamp.
    /// </summary>
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC last-updated timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
