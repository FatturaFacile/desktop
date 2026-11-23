// <copyright file="VatRegisterPrintRun.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// The audit record of one definitive print of a VAT register (stampa definitiva): the protocol range, the pages numbered progressively in the year, the content hash and the virtual stamp reference (bollatura virtuale).
/// </summary>
public class VatRegisterPrintRun : ITenantScopedEntity
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
    /// Gets or sets the register.
    /// </summary>
    public VatRegisterKind Kind { get; set; }

    /// <summary>
    /// Gets or sets the sezionale.
    /// </summary>
    public VatRegisterSection Section { get; set; }

    /// <summary>
    /// Gets or sets the register year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the first day printed.
    /// </summary>
    public DateTime FromDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the last day printed.
    /// </summary>
    public DateTime ToDate { get; set; } = DateTime.Today;

    /// <summary>
    /// Gets or sets the first protocol number printed.
    /// </summary>
    public int FirstProtocol { get; set; }

    /// <summary>
    /// Gets or sets the last protocol number printed.
    /// </summary>
    public int LastProtocol { get; set; }

    /// <summary>
    /// Gets or sets the number of lines printed.
    /// </summary>
    public int EntryCount { get; set; }

    /// <summary>
    /// Gets or sets the first page number, continuing the previous print of the year.
    /// </summary>
    public int FirstPage { get; set; }

    /// <summary>
    /// Gets or sets the last page number.
    /// </summary>
    public int LastPage { get; set; }

    /// <summary>
    /// Gets or sets the taxable total printed.
    /// </summary>
    public decimal TotalTaxable { get; set; }

    /// <summary>
    /// Gets or sets the VAT total printed.
    /// </summary>
    public decimal TotalVat { get; set; }

    /// <summary>
    /// Gets or sets the SHA-256 of the printed content, as lowercase hexadecimal.
    /// </summary>
    public string ContentSha256 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reference of the virtual stamp (bollatura virtuale) authorisation.
    /// </summary>
    public string? VirtualStampReference { get; set; }

    /// <summary>
    /// Gets or sets the user who printed.
    /// </summary>
    public string PrintedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the instant of the print in UTC.
    /// </summary>
    public DateTime PrintedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
