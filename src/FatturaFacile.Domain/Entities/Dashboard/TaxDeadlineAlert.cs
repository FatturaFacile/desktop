// <copyright file="TaxDeadlineAlert.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Dashboard;

/// <summary>
/// Represents a statutory tax or social security compliance deadline alert (e.g., Italian F24 liquidazione IVA, Esterometro, Swiss MWST-Abrechnung, AVS/AI settlement), monitoring fulfillment status and anticipated payment amounts.
/// </summary>
public sealed class TaxDeadlineAlert : Entity<Guid>, ITenantEntity
{
    private TaxDeadlineAlert()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant organization owning this tax deadline alert.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the concise title or name of the tax obligation (e.g., &quot;Liquidazione IVA Trimestrale&quot;, &quot;MWST-Abrechnung Q3&quot;).
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the detailed explanation of the fiscal filing requirements, form names, or submission guidelines.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the statutory due date by which the tax filing or payment settlement must be executed.
    /// </summary>
    public DateTime DueDate { get; private set; }

    /// <summary>
    /// Gets the estimated financial settlement liability or tax debt due for payment.
    /// </summary>
    public Money EstimatedAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the fiscal jurisdiction (Italy, Switzerland) enforcing this compliance deadline.
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this tax deadline has been marked as filed, paid, or otherwise completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp when this alert was marked as completed, or <see langword="null"/> if still pending.
    /// </summary>
    public DateTime? CompletedDateUtc { get; private set; }

    /// <summary>
    /// Creates and initializes a new <see cref="TaxDeadlineAlert"/> instance.
    /// </summary>
    /// <param name="tenantId">The unique identifier of the tenant organization.</param>
    /// <param name="title">The name or code of the tax obligation.</param>
    /// <param name="description">The regulatory details and filing requirements.</param>
    /// <param name="dueDate">The statutory payment or declaration deadline date.</param>
    /// <param name="estimatedAmount">The estimated amount due for payment.</param>
    /// <param name="jurisdiction">The tax authority jurisdiction governing the deadline.</param>
    /// <returns>A newly configured, uncompleted <see cref="TaxDeadlineAlert"/> entity.</returns>
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
    /// Marks the tax deadline as settled and records the completion timestamp in UTC.
    /// </summary>
    public void MarkCompleted()
    {
        IsCompleted = true;
        CompletedDateUtc = DateTime.UtcNow;
    }
}
