// <copyright file="SwissPayrollSettings.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Swiss payroll settings of a company: social insurance rates, ceilings and pension fund plan.
/// </summary>
public class SwissPayrollSettings : ITenantScopedEntity
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
    /// Gets or sets the employee AHV/IV/EO rate.
    /// </summary>
    public decimal AhvEmployeeRatePercent { get; set; } = 5.3m;

    /// <summary>
    /// Gets or sets the employer AHV/IV/EO rate.
    /// </summary>
    public decimal AhvEmployerRatePercent { get; set; } = 5.3m;

    /// <summary>
    /// Gets or sets the AHV administration cost charged to the employer, in percent of the contributions.
    /// </summary>
    public decimal AhvAdministrationCostPercent { get; set; }

    /// <summary>
    /// Gets or sets the employee unemployment insurance rate.
    /// </summary>
    public decimal AlvEmployeeRatePercent { get; set; } = 1.1m;

    /// <summary>
    /// Gets or sets the employer unemployment insurance rate.
    /// </summary>
    public decimal AlvEmployerRatePercent { get; set; } = 1.1m;

    /// <summary>
    /// Gets or sets the annual ALV salary ceiling.
    /// </summary>
    public decimal AlvAnnualCeiling { get; set; } = 148_200m;

    /// <summary>
    /// Gets or sets the annual UVG maximum insured salary.
    /// </summary>
    public decimal UvgAnnualCeiling { get; set; } = 148_200m;

    /// <summary>
    /// Gets or sets the default non-occupational accident rate (employee).
    /// </summary>
    public decimal NbuEmployeeRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the default occupational accident rate (employer).
    /// </summary>
    public decimal BuEmployerRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the default daily sickness insurance rate (employee).
    /// </summary>
    public decimal KtgEmployeeRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the default daily sickness insurance rate (employer).
    /// </summary>
    public decimal KtgEmployerRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the default family allowance fund rate (employer).
    /// </summary>
    public decimal FakEmployerRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the employer share of BVG contributions (at least 50 %).
    /// </summary>
    public decimal BvgEmployerSharePercent { get; set; } = 50m;

    /// <summary>
    /// Gets or sets the BVG risk and cost premium in percent of the coordinated salary.
    /// </summary>
    public decimal BvgRiskRatePercent { get; set; }

    /// <summary>
    /// Gets or sets the monthly AHV allowance for employees past the reference age.
    /// </summary>
    public decimal PensionerAllowanceMonthly { get; set; } = 1400m;

    /// <summary>
    /// Gets or sets the default work canton.
    /// </summary>
    public string DefaultWorkCanton { get; set; } = "TI";

    /// <summary>
    /// Gets or sets the AHV compensation office member number.
    /// </summary>
    public string? AhvMemberNumber { get; set; }

    /// <summary>
    /// Gets or sets the accident insurer.
    /// </summary>
    public string? UvgInsurer { get; set; }

    /// <summary>
    /// Gets or sets the pension fund.
    /// </summary>
    public string? BvgInsurer { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the last modification timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
