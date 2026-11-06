// <copyright file="ShellModules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The module view models hosted by the shell (grouped to keep the shell constructor small).
/// </summary>
/// <param name="Invoicing">The invoicing module.</param>
/// <param name="Accounting">The accounting module.</param>
/// <param name="Contacts">The contacts module.</param>
/// <param name="Products">The products module.</param>
/// <param name="Payroll">The payroll module.</param>
/// <param name="Company">The company settings module.</param>
/// <param name="Users">The users module.</param>
internal sealed record ShellModules(
    InvoicingViewModel Invoicing,
    AccountingViewModel Accounting,
    ContactsViewModel Contacts,
    ProductsViewModel Products,
    PayrollViewModel Payroll,
    CompanySettingsViewModel Company,
    UsersViewModel Users)
{
    /// <summary>
    /// Gets the working time recording module.
    /// </summary>
    public TimeTrackingViewModel? TimeTracking { get; init; }

    /// <summary>
    /// Gets the finance module (purchases, bank, VAT, fixed assets).
    /// </summary>
    public FinanceViewModel? Finance { get; init; }
}
