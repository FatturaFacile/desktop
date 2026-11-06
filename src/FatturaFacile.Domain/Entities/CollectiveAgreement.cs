// <copyright file="CollectiveAgreement.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// An Italian national collective labour agreement (CCNL) applied by the company.
/// </summary>
public class CollectiveAgreement : ITenantScopedEntity
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
    /// Gets or sets the CNEL code of the agreement (for example H011).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the agreement.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normal weekly working hours.
    /// </summary>
    public decimal WeeklyHours { get; set; } = 40m;

    /// <summary>
    /// Gets or sets the yearly salary installments (13 or 14).
    /// </summary>
    public int Installments { get; set; } = 13;

    /// <summary>
    /// Gets or sets the month in which the fourteenth installment is paid.
    /// </summary>
    public int FourteenthMonth { get; set; } = 7;

    /// <summary>
    /// Gets or sets the employer INPS rate of the sector, when different from the company default.
    /// </summary>
    public decimal? EmployerInpsRatePercent { get; set; }

    /// <summary>
    /// Gets or sets notes on the agreement (renewal, validity).
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
