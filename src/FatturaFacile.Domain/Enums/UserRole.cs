// <copyright file="UserRole.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the administrative and operational role of an application user, controlling permissions and domain feature access.
/// </summary>
public enum UserRole
{

    /// <summary>
    /// System-wide administrator with unrestricted privileges across all multi-tenant organizations, security configurations, and audit trails.
    /// </summary>
    SuperAdmin = 1,

    /// <summary>
    /// Organization administrator responsible for company setup, employee management, billing configurations, and standard business operations.
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Fiscal and accounting specialist with access to general ledger entries, tax filings, VAT liquidations, reconciliation reports, and invoice auditing.
    /// </summary>
    Accountant = 3,

    /// <summary>
    /// Mobile or on-site field operative performing vehicle inspections, workshop orders, maintenance tasks, and time-tracking on assigned jobs.
    /// </summary>
    FieldTechnician = 4,

    /// <summary>
    /// Commercial sales agent managing customer relationships, price quotations, orders, and sales commissions.
    /// </summary>
    SalesRepresentative = 5,

    /// <summary>
    /// Human resources manager administering staff contracts, cross-border permits, attendance sheets, absence approvals, and payroll data.
    /// </summary>
    HrManager = 6,

}
