// <copyright file="Permission.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Fine-grained permission checked by the application layer before executing a command or query.
/// </summary>
public enum Permission
{
    /// <summary>
    /// Read invoices, contacts and products.
    /// </summary>
    CommercialRead = 0,

    /// <summary>
    /// Create and modify contacts and products.
    /// </summary>
    MasterDataWrite = 1,

    /// <summary>
    /// Create, modify and delete draft invoices.
    /// </summary>
    InvoicesWrite = 2,

    /// <summary>
    /// Issue invoices (makes them immutable and fiscally relevant).
    /// </summary>
    InvoicesIssue = 3,

    /// <summary>
    /// Read accounting data.
    /// </summary>
    AccountingRead = 4,

    /// <summary>
    /// Post accounting entries.
    /// </summary>
    AccountingWrite = 5,

    /// <summary>
    /// Read employees and payroll.
    /// </summary>
    PayrollRead = 6,

    /// <summary>
    /// Create and modify employees and payroll.
    /// </summary>
    PayrollWrite = 7,

    /// <summary>
    /// Manage users and their roles.
    /// </summary>
    UsersManage = 8,

    /// <summary>
    /// Edit the company master data (addresses, tax identifiers, bank accounts).
    /// </summary>
    CompanySettingsManage = 9,

    /// <summary>
    /// Clock in and out and read one's own working time.
    /// </summary>
    TimeClock = 10,

    /// <summary>
    /// Read the working time records of every employee.
    /// </summary>
    TimeRead = 11,

    /// <summary>
    /// Correct working time records (audited) and manage the holiday calendar.
    /// </summary>
    TimeManage = 12,
}
