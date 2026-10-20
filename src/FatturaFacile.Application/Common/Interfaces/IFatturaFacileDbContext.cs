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
/// Defines the contract for fattura facile db context.
/// </summary>
public interface IFatturaFacileDbContext
{
    /// <summary>
    /// Gets the crm.
    /// </summary>
    public IQueryable<Client> Clients { get; }

/// <summary>
    /// Gets the contacts.
    /// </summary>
    public IQueryable<Contact> Contacts { get; }

/// <summary>
    /// Gets the suppliers.
    /// </summary>
    public IQueryable<Supplier> Suppliers { get; }

/// <summary>
    /// Gets the sales.
    /// </summary>
    public IQueryable<ItalianElectronicInvoice> ItalianElectronicInvoices { get; }

/// <summary>
    /// Gets the invoice lines.
    /// </summary>
    public IQueryable<InvoiceLine> InvoiceLines { get; }

/// <summary>
    /// Gets the swiss qr bills.
    /// </summary>
    public IQueryable<SwissQrBill> SwissQrBills { get; }

/// <summary>
    /// Gets the sales orders.
    /// </summary>
    public IQueryable<SalesOrder> SalesOrders { get; }

/// <summary>
    /// Gets the quotes.
    /// </summary>
    public IQueryable<Quote> Quotes { get; }

/// <summary>
    /// Gets the price lists.
    /// </summary>
    public IQueryable<PriceList> PriceLists { get; }

/// <summary>
    /// Gets the purchasing.
    /// </summary>
    public IQueryable<PurchaseOrder> PurchaseOrders { get; }

/// <summary>
    /// Gets the passive invoices.
    /// </summary>
    public IQueryable<PassiveInvoice> PassiveInvoices { get; }

/// <summary>
    /// Gets the iso20022 payment instructions.
    /// </summary>
    public IQueryable<Iso20022PaymentInstruction> Iso20022PaymentInstructions { get; }

/// <summary>
    /// Gets the treasury.
    /// </summary>
    public IQueryable<BankStatement> BankStatements { get; }

/// <summary>
    /// Gets the bank transaction lines.
    /// </summary>
    public IQueryable<BankTransactionLine> BankTransactionLines { get; }

/// <summary>
    /// Gets the inventory.
    /// </summary>
    public IQueryable<ItemCatalog> ItemCatalogs { get; }

/// <summary>
    /// Gets the warehouses.
    /// </summary>
    public IQueryable<Warehouse> Warehouses { get; }

/// <summary>
    /// Gets the stock movements.
    /// </summary>
    public IQueryable<StockMovement> StockMovements { get; }

/// <summary>
    /// Gets the transport documents.
    /// </summary>
    public IQueryable<TransportDocument> TransportDocuments { get; }

/// <summary>
    /// Gets the service Tickets.
    /// </summary>
    public IQueryable<SupportTicket> SupportTickets { get; }

/// <summary>
    /// Gets the technician work orders.
    /// </summary>
    public IQueryable<TechnicianWorkOrder> TechnicianWorkOrders { get; }

/// <summary>
    /// Gets the automotive.
    /// </summary>
    public IQueryable<CustomerVehicle> CustomerVehicles { get; }

/// <summary>
    /// Gets the tire hotel records.
    /// </summary>
    public IQueryable<TireHotelRecord> TireHotelRecords { get; }

/// <summary>
    /// Gets the statutory inspection records.
    /// </summary>
    public IQueryable<StatutoryInspectionRecord> StatutoryInspectionRecords { get; }

/// <summary>
    /// Gets the fleet.
    /// </summary>
    public IQueryable<RentalVehicle> RentalVehicles { get; }

/// <summary>
    /// Gets the rental bookings.
    /// </summary>
    public IQueryable<RentalBooking> RentalBookings { get; }

/// <summary>
    /// Gets the damage appraisals.
    /// </summary>
    public IQueryable<DamageAppraisal> DamageAppraisals { get; }

/// <summary>
    /// Gets the construction.
    /// </summary>
    public IQueryable<ConstructionSite> ConstructionSites { get; }

/// <summary>
    /// Gets the bill of quantities items.
    /// </summary>
    public IQueryable<BillOfQuantitiesItem> BillOfQuantitiesItems { get; }

/// <summary>
    /// Gets the progress billing sals.
    /// </summary>
    public IQueryable<ProgressBillingSal> ProgressBillingSals { get; }

/// <summary>
    /// Gets the declarations of conformity.
    /// </summary>
    public IQueryable<DeclarationOfConformity> DeclarationsOfConformity { get; }

/// <summary>
    /// Gets the advisory.
    /// </summary>
    public IQueryable<ClientMatter> ClientMatters { get; }

/// <summary>
    /// Gets the billable time entries.
    /// </summary>
    public IQueryable<BillableTimeEntry> BillableTimeEntries { get; }

/// <summary>
    /// Gets the professional fee notes.
    /// </summary>
    public IQueryable<ProfessionalFeeNote> ProfessionalFeeNotes { get; }

/// <summary>
    /// Gets the manufacturing.
    /// </summary>
    public IQueryable<BillOfMaterials> BillOfMaterials { get; }

/// <summary>
    /// Gets the bom lines.
    /// </summary>
    public IQueryable<BomLine> BomLines { get; }

/// <summary>
    /// Gets the work centers.
    /// </summary>
    public IQueryable<WorkCenter> WorkCenters { get; }

/// <summary>
    /// Gets the production orders.
    /// </summary>
    public IQueryable<ProductionOrder> ProductionOrders { get; }

/// <summary>
    /// Gets the retail Pos.
    /// </summary>
    public IQueryable<PosTerminalSession> PosTerminalSessions { get; }

/// <summary>
    /// Gets the retail receipts.
    /// </summary>
    public IQueryable<RetailReceipt> RetailReceipts { get; }

/// <summary>
    /// Gets the time Tracking.
    /// </summary>
    public IQueryable<TimeEntry> TimeEntries { get; }

/// <summary>
    /// Gets the absence records.
    /// </summary>
    public IQueryable<AbsenceRecord> AbsenceRecords { get; }

/// <summary>
    /// Gets the payroll &amp; HR.
    /// </summary>
    public IQueryable<Employee> Employees { get; }

/// <summary>
    /// Gets the payroll slip summaries.
    /// </summary>
    public IQueryable<PayrollSlipSummary> PayrollSlipSummaries { get; }

/// <summary>
    /// Gets the accounting.
    /// </summary>
    public IQueryable<GeneralLedgerAccount> GeneralLedgerAccounts { get; }

/// <summary>
    /// Gets the general ledger entries.
    /// </summary>
    public IQueryable<GeneralLedgerEntry> GeneralLedgerEntries { get; }

/// <summary>
    /// Gets the general ledger lines.
    /// </summary>
    public IQueryable<GeneralLedgerLine> GeneralLedgerLines { get; }

/// <summary>
    /// Gets the vat settlement periods.
    /// </summary>
    public IQueryable<VatSettlementPeriod> VatSettlementPeriods { get; }

/// <summary>
    /// Gets the dashboard &amp; Admin.
    /// </summary>
    public IQueryable<ExecutiveKpiSummary> ExecutiveKpiSummaries { get; }

/// <summary>
    /// Gets the tax deadline alerts.
    /// </summary>
    public IQueryable<TaxDeadlineAlert> TaxDeadlineAlerts { get; }

/// <summary>
    /// Gets the tenants.
    /// </summary>
    public IQueryable<Tenant> Tenants { get; }

/// <summary>
    /// Gets the branches.
    /// </summary>
    public IQueryable<Branch> Branches { get; }

/// <summary>
    /// Gets the users.
    /// </summary>
    public IQueryable<User> Users { get; }

/// <summary>
    /// Gets the exchange rates.
    /// </summary>
    public IQueryable<ExchangeRate> ExchangeRates { get; }

/// <summary>
    /// Gets the app translations.
    /// </summary>
    public IQueryable<AppTranslation> AppTranslations { get; }

/// <summary>
    /// Save Changes Async.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
