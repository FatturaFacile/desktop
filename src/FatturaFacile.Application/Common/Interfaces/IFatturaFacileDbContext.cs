// <copyright file="IFatturaFacileDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities.Accounting;
using FatturaFacile.Domain.Entities.Administration;
using FatturaFacile.Domain.Entities.Advisory;
using FatturaFacile.Domain.Entities.Automotive;
using FatturaFacile.Domain.Entities.Construction;
using FatturaFacile.Domain.Entities.Crm;
using FatturaFacile.Domain.Entities.Dashboard;
using FatturaFacile.Domain.Entities.FleetRental;
using FatturaFacile.Domain.Entities.Inventory;
using FatturaFacile.Domain.Entities.Manufacturing;
using FatturaFacile.Domain.Entities.PayrollHr;
using FatturaFacile.Domain.Entities.Purchasing;
using FatturaFacile.Domain.Entities.RetailPos;
using FatturaFacile.Domain.Entities.Sales;
using FatturaFacile.Domain.Entities.ServiceTickets;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Entities.Treasury;

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Defines the database context abstraction exposing aggregate queryable entity sets across all core domain modules.
/// </summary>
public interface IFatturaFacileDbContext
{
    /// <summary>
    /// Gets queryable clients and customer account records.
    /// </summary>
    public IQueryable<Client> Clients { get; }

    /// <summary>
    /// Gets queryable individual contact persons linked to clients or suppliers.
    /// </summary>
    public IQueryable<Contact> Contacts { get; }

    /// <summary>
    /// Gets queryable vendor and supplier entities.
    /// </summary>
    public IQueryable<Supplier> Suppliers { get; }

    /// <summary>
    /// Gets queryable Italian electronic invoice (FatturaPA) records.
    /// </summary>
    public IQueryable<ItalianElectronicInvoice> ItalianElectronicInvoices { get; }

    /// <summary>
    /// Gets queryable individual line items associated with electronic invoices.
    /// </summary>
    public IQueryable<InvoiceLine> InvoiceLines { get; }

    /// <summary>
    /// Gets queryable Swiss QR-bill payment and invoicing records.
    /// </summary>
    public IQueryable<SwissQrBill> SwissQrBills { get; }

    /// <summary>
    /// Gets queryable sales order documents and commitments.
    /// </summary>
    public IQueryable<SalesOrder> SalesOrders { get; }

    /// <summary>
    /// Gets queryable price quotations and commercial estimates issued to clients.
    /// </summary>
    public IQueryable<Quote> Quotes { get; }

    /// <summary>
    /// Gets queryable custom price lists and item pricing matrices.
    /// </summary>
    public IQueryable<PriceList> PriceLists { get; }

    /// <summary>
    /// Gets queryable purchasing orders sent to suppliers.
    /// </summary>
    public IQueryable<PurchaseOrder> PurchaseOrders { get; }

    /// <summary>
    /// Gets queryable passive purchase invoices received from vendors.
    /// </summary>
    public IQueryable<PassiveInvoice> PassiveInvoices { get; }

    /// <summary>
    /// Gets queryable ISO 20022 SEPA payment execution instructions.
    /// </summary>
    public IQueryable<Iso20022PaymentInstruction> Iso20022PaymentInstructions { get; }

    /// <summary>
    /// Gets queryable reconciled bank statement documents.
    /// </summary>
    public IQueryable<BankStatement> BankStatements { get; }

    /// <summary>
    /// Gets queryable individual financial movement lines parsed from bank statements.
    /// </summary>
    public IQueryable<BankTransactionLine> BankTransactionLines { get; }

    /// <summary>
    /// Gets queryable inventory catalog items, services, and raw materials.
    /// </summary>
    public IQueryable<ItemCatalog> ItemCatalogs { get; }

    /// <summary>
    /// Gets queryable physical warehouses, stock storage zones, and depots.
    /// </summary>
    public IQueryable<Warehouse> Warehouses { get; }

    /// <summary>
    /// Gets queryable audit records of stock movements, adjustments, and transfers.
    /// </summary>
    public IQueryable<StockMovement> StockMovements { get; }

    /// <summary>
    /// Gets queryable transport documents (DDT / Bolla di Accompagnamento).
    /// </summary>
    public IQueryable<TransportDocument> TransportDocuments { get; }

    /// <summary>
    /// Gets queryable technical support cases and customer assistance tickets.
    /// </summary>
    public IQueryable<SupportTicket> SupportTickets { get; }

    /// <summary>
    /// Gets queryable field technician intervention and repair work orders.
    /// </summary>
    public IQueryable<TechnicianWorkOrder> TechnicianWorkOrders { get; }

    /// <summary>
    /// Gets queryable customer vehicles registered for automotive services.
    /// </summary>
    public IQueryable<CustomerVehicle> CustomerVehicles { get; }

    /// <summary>
    /// Gets queryable seasonal tire hotel deposit and storage records.
    /// </summary>
    public IQueryable<TireHotelRecord> TireHotelRecords { get; }

    /// <summary>
    /// Gets queryable vehicle roadworthiness inspection and statutory revision records (Revisione / MFK).
    /// </summary>
    public IQueryable<StatutoryInspectionRecord> StatutoryInspectionRecords { get; }

    /// <summary>
    /// Gets queryable rental fleet vehicles and asset management records.
    /// </summary>
    public IQueryable<RentalVehicle> RentalVehicles { get; }

    /// <summary>
    /// Gets queryable commercial vehicle rental reservations and bookings.
    /// </summary>
    public IQueryable<RentalBooking> RentalBookings { get; }

    /// <summary>
    /// Gets queryable vehicle return damage appraisals and inspection assessments.
    /// </summary>
    public IQueryable<DamageAppraisal> DamageAppraisals { get; }

    /// <summary>
    /// Gets queryable construction yard projects and building sites (Cantieri).
    /// </summary>
    public IQueryable<ConstructionSite> ConstructionSites { get; }

    /// <summary>
    /// Gets queryable bill of quantities item lines (Computo Metrico Estimativo).
    /// </summary>
    public IQueryable<BillOfQuantitiesItem> BillOfQuantitiesItems { get; }

    /// <summary>
    /// Gets queryable construction progress billing certifications (Stato Avanzamento Lavori - SAL).
    /// </summary>
    public IQueryable<ProgressBillingSal> ProgressBillingSals { get; }

    /// <summary>
    /// Gets queryable technical plant safety and regulatory declarations of conformity (DiCo).
    /// </summary>
    public IQueryable<DeclarationOfConformity> DeclarationsOfConformity { get; }

    /// <summary>
    /// Gets queryable advisory and legal or consulting practice case matters.
    /// </summary>
    public IQueryable<ClientMatter> ClientMatters { get; }

    /// <summary>
    /// Gets queryable professional advisory billable activities and time entries.
    /// </summary>
    public IQueryable<BillableTimeEntry> BillableTimeEntries { get; }

    /// <summary>
    /// Gets queryable professional fee notes and pro-forma statements (Parcelle / Note Pro-Forma).
    /// </summary>
    public IQueryable<ProfessionalFeeNote> ProfessionalFeeNotes { get; }

    /// <summary>
    /// Gets queryable bill of materials (BOM) definitions for manufacturing and assembly.
    /// </summary>
    public IQueryable<BillOfMaterials> BillOfMaterials { get; }

    /// <summary>
    /// Gets queryable recipe and bill of material component line items.
    /// </summary>
    public IQueryable<BomLine> BomLines { get; }

    /// <summary>
    /// Gets queryable shop-floor work centers, production machinery, and workstations.
    /// </summary>
    public IQueryable<WorkCenter> WorkCenters { get; }

    /// <summary>
    /// Gets queryable manufacturing production orders and job shop dispatches.
    /// </summary>
    public IQueryable<ProductionOrder> ProductionOrders { get; }

    /// <summary>
    /// Gets queryable Point of Sale (POS) register sessions and cashier shifts.
    /// </summary>
    public IQueryable<PosTerminalSession> PosTerminalSessions { get; }

    /// <summary>
    /// Gets queryable retail fiscal receipts and RT electronic transmission records (Scontrini Fiscali).
    /// </summary>
    public IQueryable<RetailReceipt> RetailReceipts { get; }

    /// <summary>
    /// Gets queryable staff clock-in / clock-out attendance and time tracking entries.
    /// </summary>
    public IQueryable<TimeEntry> TimeEntries { get; }

    /// <summary>
    /// Gets queryable employee absence and leave records (vacation, sickness, permits).
    /// </summary>
    public IQueryable<AbsenceRecord> AbsenceRecords { get; }

    /// <summary>
    /// Gets queryable employee master data records and employment contracts.
    /// </summary>
    public IQueryable<Employee> Employees { get; }

    /// <summary>
    /// Gets queryable periodic employee payroll slip and withholding summaries (Cedolini Paga).
    /// </summary>
    public IQueryable<PayrollSlipSummary> PayrollSlipSummaries { get; }

    /// <summary>
    /// Gets queryable general ledger chart of accounts (Piano dei Conti).
    /// </summary>
    public IQueryable<GeneralLedgerAccount> GeneralLedgerAccounts { get; }

    /// <summary>
    /// Gets queryable journal entry header transactions (Prima Nota / Libro Giornale).
    /// </summary>
    public IQueryable<GeneralLedgerEntry> GeneralLedgerEntries { get; }

    /// <summary>
    /// Gets queryable double-entry debit and credit lines comprising journal entries.
    /// </summary>
    public IQueryable<GeneralLedgerLine> GeneralLedgerLines { get; }

    /// <summary>
    /// Gets queryable VAT settlement calculation periods and liquidations (Liquidazioni IVA).
    /// </summary>
    public IQueryable<VatSettlementPeriod> VatSettlementPeriods { get; }

    /// <summary>
    /// Gets queryable aggregated executive KPI snapshots and dashboard metric summaries.
    /// </summary>
    public IQueryable<ExecutiveKpiSummary> ExecutiveKpiSummaries { get; }

    /// <summary>
    /// Gets queryable fiscal calendar deadline alerts and statutory compliance milestones.
    /// </summary>
    public IQueryable<TaxDeadlineAlert> TaxDeadlineAlerts { get; }

    /// <summary>
    /// Gets queryable multi-tenant organization units registered in the system.
    /// </summary>
    public IQueryable<Tenant> Tenants { get; }

    /// <summary>
    /// Gets queryable physical branches, corporate locations, and local subsidiaries.
    /// </summary>
    public IQueryable<Branch> Branches { get; }

    /// <summary>
    /// Gets queryable application operator and security user accounts.
    /// </summary>
    public IQueryable<User> Users { get; }

    /// <summary>
    /// Gets queryable foreign currency exchange rates across historical conversion dates.
    /// </summary>
    public IQueryable<ExchangeRate> ExchangeRates { get; }

    /// <summary>
    /// Gets queryable localized application translation dictionaries and overrides.
    /// </summary>
    public IQueryable<AppTranslation> AppTranslations { get; }

    /// <summary>
    /// Saves all changes tracked by the underlying persistence context asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that yields the count of state entries persisted.</returns>
    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
