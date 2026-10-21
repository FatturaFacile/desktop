// <copyright file="TaxDeadlineAlert.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Dashboard;

/// <summary>
/// Represents tax deadline alert.
/// </summary>
public sealed class TaxDeadlineAlert : Entity<Guid>, ITenantEntity
{
    private TaxDeadlineAlert()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the title.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the due date.
    /// </summary>
    public DateTime DueDate { get; private set; }

    /// <summary>
    /// Gets the estimated amount.
    /// </summary>
    public Money EstimatedAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the jurisdiction.
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets a value indicating whether completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Gets the completed date utc.
    /// </summary>
    public DateTime? CompletedDateUtc { get; private set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="title">The title.</param>
    /// <param name="description">The description.</param>
    /// <param name="dueDate">The due date.</param>
    /// <param name="estimatedAmount">The estimated amount.</param>
    /// <param name="jurisdiction">The jurisdiction.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static TaxDeadlineAlert Create(
        Guid tenantId,
        string title,
        string description,
        DateTime dueDate,
        Money estimatedAmount,
        Jurisdiction jurisdiction)
    {
        return new TaxDeadlineAlert
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = title,
            Description = description,
            DueDate = dueDate,
            EstimatedAmount = estimatedAmount,
            Jurisdiction = jurisdiction,
            IsCompleted = false,
        };
    }

    /// <summary>
    /// Mark Completed.
    /// </summary>
    public void MarkCompleted()
    {
        IsCompleted = true;
        CompletedDateUtc = DateTime.UtcNow;
    }
}
