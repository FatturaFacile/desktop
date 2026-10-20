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

public interface IFatturaFacileDbContext
{
    /// <summary>
    /// Crm
    /// </summary>
    public IQueryable<Client> Clients { get; }
    public IQueryable<Contact> Contacts { get; }
    public IQueryable<Supplier> Suppliers { get; }

    /// <summary>
    /// Sales
    /// </summary>
    public IQueryable<ItalianElectronicInvoice> ItalianElectronicInvoices { get; }
    public IQueryable<InvoiceLine> InvoiceLines { get; }
    public IQueryable<SwissQrBill> SwissQrBills { get; }
    public IQueryable<SalesOrder> SalesOrders { get; }
    public IQueryable<Quote> Quotes { get; }
    public IQueryable<PriceList> PriceLists { get; }

    /// <summary>
    /// Purchasing
    /// </summary>
    public IQueryable<PurchaseOrder> PurchaseOrders { get; }
    public IQueryable<PassiveInvoice> PassiveInvoices { get; }
    public IQueryable<Iso20022PaymentInstruction> Iso20022PaymentInstructions { get; }

    /// <summary>
    /// Treasury
    /// </summary>
    public IQueryable<BankStatement> BankStatements { get; }
    public IQueryable<BankTransactionLine> BankTransactionLines { get; }

    /// <summary>
    /// Inventory
    /// </summary>
    public IQueryable<ItemCatalog> ItemCatalogs { get; }
    public IQueryable<Warehouse> Warehouses { get; }
    public IQueryable<StockMovement> StockMovements { get; }
    public IQueryable<TransportDocument> TransportDocuments { get; }

    /// <summary>
    /// Service Tickets
    /// </summary>
    public IQueryable<SupportTicket> SupportTickets { get; }
    public IQueryable<TechnicianWorkOrder> TechnicianWorkOrders { get; }

    /// <summary>
    /// Automotive
    /// </summary>
    public IQueryable<CustomerVehicle> CustomerVehicles { get; }
    public IQueryable<TireHotelRecord> TireHotelRecords { get; }
    public IQueryable<StatutoryInspectionRecord> StatutoryInspectionRecords { get; }

    /// <summary>
    /// Fleet
    /// </summary>
    public IQueryable<RentalVehicle> RentalVehicles { get; }
    public IQueryable<RentalBooking> RentalBookings { get; }
    public IQueryable<DamageAppraisal> DamageAppraisals { get; }

    /// <summary>
    /// Construction
    /// </summary>
    public IQueryable<ConstructionSite> ConstructionSites { get; }
    public IQueryable<BillOfQuantitiesItem> BillOfQuantitiesItems { get; }
    public IQueryable<ProgressBillingSal> ProgressBillingSals { get; }
    public IQueryable<DeclarationOfConformity> DeclarationsOfConformity { get; }

    /// <summary>
    /// Advisory
    /// </summary>
    public IQueryable<ClientMatter> ClientMatters { get; }
    public IQueryable<BillableTimeEntry> BillableTimeEntries { get; }
    public IQueryable<ProfessionalFeeNote> ProfessionalFeeNotes { get; }

    /// <summary>
    /// Manufacturing
    /// </summary>
    public IQueryable<BillOfMaterials> BillOfMaterials { get; }
    public IQueryable<BomLine> BomLines { get; }
    public IQueryable<WorkCenter> WorkCenters { get; }
    public IQueryable<ProductionOrder> ProductionOrders { get; }

    /// <summary>
    /// Retail Pos
    /// </summary>
    public IQueryable<PosTerminalSession> PosTerminalSessions { get; }
    public IQueryable<RetailReceipt> RetailReceipts { get; }

    /// <summary>
    /// Time Tracking
    /// </summary>
    public IQueryable<TimeEntry> TimeEntries { get; }
    public IQueryable<AbsenceRecord> AbsenceRecords { get; }

    /// <summary>
    /// Payroll & HR
    /// </summary>
    public IQueryable<Employee> Employees { get; }
    public IQueryable<PayrollSlipSummary> PayrollSlipSummaries { get; }

    /// <summary>
    /// Accounting
    /// </summary>
    public IQueryable<GeneralLedgerAccount> GeneralLedgerAccounts { get; }
    public IQueryable<GeneralLedgerEntry> GeneralLedgerEntries { get; }
    public IQueryable<GeneralLedgerLine> GeneralLedgerLines { get; }
    public IQueryable<VatSettlementPeriod> VatSettlementPeriods { get; }

    /// <summary>
    /// Dashboard & Admin
    /// </summary>
    public IQueryable<ExecutiveKpiSummary> ExecutiveKpiSummaries { get; }
    public IQueryable<TaxDeadlineAlert> TaxDeadlineAlerts { get; }
    public IQueryable<Tenant> Tenants { get; }
    public IQueryable<Branch> Branches { get; }
    public IQueryable<User> Users { get; }
    public IQueryable<ExchangeRate> ExchangeRates { get; }
    public IQueryable<AppTranslation> AppTranslations { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
