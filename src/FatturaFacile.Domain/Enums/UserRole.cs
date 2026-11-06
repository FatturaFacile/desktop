// <copyright file="UserRole.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Role a user holds within a single tenant (company).
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Read-only access to commercial and accounting data (no payroll).
    /// </summary>
    Viewer = 0,

    /// <summary>
    /// Sales: contacts, products and invoices (draft and issue).
    /// </summary>
    Sales = 1,

    /// <summary>
    /// Accountant: everything except payroll and user management.
    /// </summary>
    Accountant = 2,

    /// <summary>
    /// Payroll officer: employees and payroll, read access to the rest.
    /// </summary>
    Payroll = 3,

    /// <summary>
    /// Tenant administrator: all permissions in the tenant, including user management.
    /// </summary>
    Administrator = 4,
}
