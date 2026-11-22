// <copyright file="ShellModules.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.ViewModels.Copilot;
using FatturaFacile.Wpf.ViewModels.PhysicalInventory;

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

    /// <summary>
    /// Gets the data protection module.
    /// </summary>
    public PrivacyViewModel? Privacy { get; init; }

    /// <summary>
    /// Gets the Executive DSS cockpit.
    /// </summary>
    public ExecutiveDssViewModel? Dss { get; init; }

    /// <summary>
    /// Gets the project profitability and earned value view (Mod 082).
    /// </summary>
    public ProjectEvmViewModel? ProjectEvm { get; init; }

    /// <summary>
    /// Gets the industrial and analytical cost accounting view (Mod 018).
    /// </summary>
    public AnalyticalAccountingViewModel? Analytical { get; init; }

    /// <summary>
    /// Gets the customer lifecycle view (issue #193).
    /// </summary>
    public CustomerLifecycleViewModel? CustomerLifecycle { get; init; }

    /// <summary>
    /// Gets the guided gelato weighing station (Mod 398).
    /// </summary>
    public GelatoWeighingViewModel? GelatoWeighing { get; init; }

    /// <summary>
    /// Gets the Field Service Management module: dispatcher board, maintenance contracts, work orders and equipment history.
    /// </summary>
    public FieldServiceViewModel? FieldService { get; init; }

    /// <summary>
    /// Gets the support ticket workbench (Mod 083).
    /// </summary>
    public HelpdeskViewModel? Helpdesk { get; init; }

    /// <summary>
    /// Gets the SLA monitoring view (Mod 083).
    /// </summary>
    public SlaMonitorViewModel? SlaMonitor { get; init; }

    /// <summary>
    /// Gets the resource planning and capacity saturation module (Mod 079).
    /// </summary>
    public ResourcePlanningViewModel? ResourcePlanning { get; init; }

    /// <summary>
    /// Gets the Mod 061 production engineering module (work centres, routings, OEE).
    /// </summary>
    public ManufacturingViewModel? Manufacturing { get; init; }

    /// <summary>
    /// Gets the MES operator board.
    /// </summary>
    public OperatorBoardViewModel? OperatorBoard { get; init; }

    /// <summary>
    /// Gets the security and audit trail module.
    /// </summary>
    public SecurityViewModel? Security { get; init; }

    /// <summary>
    /// Gets the web-api synchronization controls.
    /// </summary>
    public SyncViewModel? Sync { get; init; }

    /// <summary>
    /// Gets the AI copilot panel (pending suggestions, insights and offline decisions).
    /// </summary>
    public AiCopilotPanelViewModel? AiCopilot { get; init; }

    /// <summary>
    /// Gets the engineering and shop-floor workbench.
    /// </summary>
    public WorkbenchViewModel? Workbench { get; init; }

    /// <summary>
    /// Gets the physical inventory module (ABC analysis, cycle counting, reconciliation and approval).
    /// </summary>
    public PhysicalInventoryViewModel? PhysicalInventory { get; init; }

    /// <summary>
    /// Gets the ERP Copilot drawer.
    /// </summary>
    public CopilotViewModel? Copilot { get; init; }
}
