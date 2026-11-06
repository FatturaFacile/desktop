// <copyright file="ProcessingActivity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// An entry of the register of processing activities (art. 12 nLPD, art. 30 GDPR).
/// </summary>
public class ProcessingActivity : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the company (tenant) identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the name of the processing.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the purpose of the processing.
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the legal basis.
    /// </summary>
    public string? LegalBasis { get; set; }

    /// <summary>
    /// Gets or sets the categories of data subjects.
    /// </summary>
    public string? DataSubjects { get; set; }

    /// <summary>
    /// Gets or sets the categories of personal data.
    /// </summary>
    public string? DataCategories { get; set; }

    /// <summary>
    /// Gets or sets the categories of recipients.
    /// </summary>
    public string? Recipients { get; set; }

    /// <summary>
    /// Gets or sets the transfers abroad and their guarantees.
    /// </summary>
    public string? ForeignTransfers { get; set; }

    /// <summary>
    /// Gets or sets the retention period.
    /// </summary>
    public string? RetentionPeriod { get; set; }

    /// <summary>
    /// Gets or sets the technical and organisational security measures.
    /// </summary>
    public string? SecurityMeasures { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
