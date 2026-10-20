using System.Linq.Expressions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
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
using FatturaFacile.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
    /// Represents fattura facile db context.
    /// </summary>
/// <param name="options">The options.</param>
/// <param name="tenantProvider">The tenant provider.</param>
/// <param name="currentUserService">The current user service.</param>
    public sealed class FatturaFacileDbContext(
    DbContextOptions<FatturaFacileDbContext> options,
    ITenantProvider tenantProvider,
    ICurrentUserService currentUserService) : DbContext(options), IFatturaFacileDbContext, IUnitOfWork
    {
    private readonly ITenantProvider _tenantProvider = tenantProvider;

private readonly ICurrentUserService _currentUserService = currentUserService;

/// <summary>
    /// Gets the crm.
    /// </summary>
public DbSet<Client> Clients => Set<Client>();

/// <summary>
    /// Gets the contacts.
    /// </summary>
public DbSet<Contact> Contacts => Set<Contact>();

/// <summary>
    /// Gets the suppliers.
    /// </summary>
public DbSet<Supplier> Suppliers => Set<Supplier>();

/// <summary>
    /// Gets the sales.
    /// </summary>
public DbSet<ItalianElectronicInvoice> ItalianElectronicInvoices => Set<ItalianElectronicInvoice>();

/// <summary>
    /// Gets the invoice lines.
    /// </summary>
public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

/// <summary>
    /// Gets the swiss qr bills.
    /// </summary>
public DbSet<SwissQrBill> SwissQrBills => Set<SwissQrBill>();

/// <summary>
    /// Gets the sales orders.
    /// </summary>
public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

/// <summary>
    /// Gets the quotes.
    /// </summary>
public DbSet<Quote> Quotes => Set<Quote>();

/// <summary>
    /// Gets the price lists.
    /// </summary>
public DbSet<PriceList> PriceLists => Set<PriceList>();

/// <summary>
    /// Gets the purchasing.
    /// </summary>
public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

/// <summary>
    /// Gets the passive invoices.
    /// </summary>
public DbSet<PassiveInvoice> PassiveInvoices => Set<PassiveInvoice>();

/// <summary>
    /// Gets the iso20022 payment instructions.
    /// </summary>
public DbSet<Iso20022PaymentInstruction> Iso20022PaymentInstructions => Set<Iso20022PaymentInstruction>();

/// <summary>
    /// Gets the treasury.
    /// </summary>
public DbSet<BankStatement> BankStatements => Set<BankStatement>();

/// <summary>
    /// Gets the bank transaction lines.
    /// </summary>
public DbSet<BankTransactionLine> BankTransactionLines => Set<BankTransactionLine>();

/// <summary>
    /// Gets the inventory.
    /// </summary>
public DbSet<ItemCatalog> ItemCatalogs => Set<ItemCatalog>();

/// <summary>
    /// Gets the warehouses.
    /// </summary>
public DbSet<Warehouse> Warehouses => Set<Warehouse>();

/// <summary>
    /// Gets the stock movements.
    /// </summary>
public DbSet<StockMovement> StockMovements => Set<StockMovement>();

/// <summary>
    /// Gets the transport documents.
    /// </summary>
public DbSet<TransportDocument> TransportDocuments => Set<TransportDocument>();

/// <summary>
    /// Gets the service Tickets.
    /// </summary>
public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

/// <summary>
    /// Gets the technician work orders.
    /// </summary>
public DbSet<TechnicianWorkOrder> TechnicianWorkOrders => Set<TechnicianWorkOrder>();

/// <summary>
    /// Gets the automotive.
    /// </summary>
public DbSet<CustomerVehicle> CustomerVehicles => Set<CustomerVehicle>();

/// <summary>
    /// Gets the tire hotel records.
    /// </summary>
public DbSet<TireHotelRecord> TireHotelRecords => Set<TireHotelRecord>();

/// <summary>
    /// Gets the statutory inspection records.
    /// </summary>
public DbSet<StatutoryInspectionRecord> StatutoryInspectionRecords => Set<StatutoryInspectionRecord>();

/// <summary>
    /// Gets the fleet Rental.
    /// </summary>
public DbSet<RentalVehicle> RentalVehicles => Set<RentalVehicle>();

/// <summary>
    /// Gets the rental bookings.
    /// </summary>
public DbSet<RentalBooking> RentalBookings => Set<RentalBooking>();

/// <summary>
    /// Gets the damage appraisals.
    /// </summary>
public DbSet<DamageAppraisal> DamageAppraisals => Set<DamageAppraisal>();

/// <summary>
    /// Gets the construction.
    /// </summary>
public DbSet<ConstructionSite> ConstructionSites => Set<ConstructionSite>();

/// <summary>
    /// Gets the bill of quantities items.
    /// </summary>
public DbSet<BillOfQuantitiesItem> BillOfQuantitiesItems => Set<BillOfQuantitiesItem>();

/// <summary>
    /// Gets the progress billing sals.
    /// </summary>
public DbSet<ProgressBillingSal> ProgressBillingSals => Set<ProgressBillingSal>();

/// <summary>
    /// Gets the declarations of conformity.
    /// </summary>
public DbSet<DeclarationOfConformity> DeclarationsOfConformity => Set<DeclarationOfConformity>();

/// <summary>
    /// Gets the advisory.
    /// </summary>
public DbSet<ClientMatter> ClientMatters => Set<ClientMatter>();

/// <summary>
    /// Gets the billable time entries.
    /// </summary>
public DbSet<BillableTimeEntry> BillableTimeEntries => Set<BillableTimeEntry>();

/// <summary>
    /// Gets the professional fee notes.
    /// </summary>
public DbSet<ProfessionalFeeNote> ProfessionalFeeNotes => Set<ProfessionalFeeNote>();

/// <summary>
    /// Gets the manufacturing.
    /// </summary>
public DbSet<BillOfMaterials> BillOfMaterials => Set<BillOfMaterials>();

/// <summary>
    /// Gets the bom lines.
    /// </summary>
public DbSet<BomLine> BomLines => Set<BomLine>();

/// <summary>
    /// Gets the work centers.
    /// </summary>
public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();

/// <summary>
    /// Gets the production orders.
    /// </summary>
public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

/// <summary>
    /// Gets the retail Pos.
    /// </summary>
public DbSet<PosTerminalSession> PosTerminalSessions => Set<PosTerminalSession>();

/// <summary>
    /// Gets the retail receipts.
    /// </summary>
public DbSet<RetailReceipt> RetailReceipts => Set<RetailReceipt>();

/// <summary>
    /// Gets the time Tracking.
    /// </summary>
public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

/// <summary>
    /// Gets the absence records.
    /// </summary>
public DbSet<AbsenceRecord> AbsenceRecords => Set<AbsenceRecord>();

/// <summary>
    /// Gets the payroll &amp; HR.
    /// </summary>
public DbSet<Employee> Employees => Set<Employee>();

/// <summary>
    /// Gets the payroll slip summaries.
    /// </summary>
public DbSet<PayrollSlipSummary> PayrollSlipSummaries => Set<PayrollSlipSummary>();

/// <summary>
    /// Gets the accounting.
    /// </summary>
public DbSet<GeneralLedgerAccount> GeneralLedgerAccounts => Set<GeneralLedgerAccount>();

/// <summary>
    /// Gets the general ledger entries.
    /// </summary>
public DbSet<GeneralLedgerEntry> GeneralLedgerEntries => Set<GeneralLedgerEntry>();

/// <summary>
    /// Gets the general ledger lines.
    /// </summary>
public DbSet<GeneralLedgerLine> GeneralLedgerLines => Set<GeneralLedgerLine>();

/// <summary>
    /// Gets the vat settlement periods.
    /// </summary>
public DbSet<VatSettlementPeriod> VatSettlementPeriods => Set<VatSettlementPeriod>();

/// <summary>
    /// Gets the dashboard &amp; Admin.
    /// </summary>
public DbSet<ExecutiveKpiSummary> ExecutiveKpiSummaries => Set<ExecutiveKpiSummary>();

/// <summary>
    /// Gets the tax deadline alerts.
    /// </summary>
public DbSet<TaxDeadlineAlert> TaxDeadlineAlerts => Set<TaxDeadlineAlert>();

/// <summary>
    /// Gets the tenants.
    /// </summary>
public DbSet<Tenant> Tenants => Set<Tenant>();

/// <summary>
    /// Gets the branches.
    /// </summary>
public DbSet<Branch> Branches => Set<Branch>();

/// <summary>
    /// Gets the users.
    /// </summary>
public DbSet<User> Users => Set<User>();

/// <summary>
    /// Gets the exchange rates.
    /// </summary>
public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

/// <summary>
    /// Gets the app translations.
    /// </summary>
public DbSet<AppTranslation> AppTranslations => Set<AppTranslation>();

IQueryable<Client> IFatturaFacileDbContext.Clients => Clients;

IQueryable<Contact> IFatturaFacileDbContext.Contacts => Contacts;

IQueryable<Supplier> IFatturaFacileDbContext.Suppliers => Suppliers;

IQueryable<ItalianElectronicInvoice> IFatturaFacileDbContext.ItalianElectronicInvoices => ItalianElectronicInvoices;

IQueryable<InvoiceLine> IFatturaFacileDbContext.InvoiceLines => InvoiceLines;

IQueryable<SwissQrBill> IFatturaFacileDbContext.SwissQrBills => SwissQrBills;

IQueryable<SalesOrder> IFatturaFacileDbContext.SalesOrders => SalesOrders;

IQueryable<Quote> IFatturaFacileDbContext.Quotes => Quotes;

IQueryable<PriceList> IFatturaFacileDbContext.PriceLists => PriceLists;

IQueryable<PurchaseOrder> IFatturaFacileDbContext.PurchaseOrders => PurchaseOrders;

IQueryable<PassiveInvoice> IFatturaFacileDbContext.PassiveInvoices => PassiveInvoices;

IQueryable<Iso20022PaymentInstruction> IFatturaFacileDbContext.Iso20022PaymentInstructions => Iso20022PaymentInstructions;

IQueryable<BankStatement> IFatturaFacileDbContext.BankStatements => BankStatements;

IQueryable<BankTransactionLine> IFatturaFacileDbContext.BankTransactionLines => BankTransactionLines;

IQueryable<ItemCatalog> IFatturaFacileDbContext.ItemCatalogs => ItemCatalogs;

IQueryable<Warehouse> IFatturaFacileDbContext.Warehouses => Warehouses;

IQueryable<StockMovement> IFatturaFacileDbContext.StockMovements => StockMovements;

IQueryable<TransportDocument> IFatturaFacileDbContext.TransportDocuments => TransportDocuments;

IQueryable<SupportTicket> IFatturaFacileDbContext.SupportTickets => SupportTickets;

IQueryable<TechnicianWorkOrder> IFatturaFacileDbContext.TechnicianWorkOrders => TechnicianWorkOrders;

IQueryable<CustomerVehicle> IFatturaFacileDbContext.CustomerVehicles => CustomerVehicles;

IQueryable<TireHotelRecord> IFatturaFacileDbContext.TireHotelRecords => TireHotelRecords;

IQueryable<StatutoryInspectionRecord> IFatturaFacileDbContext.StatutoryInspectionRecords => StatutoryInspectionRecords;

IQueryable<RentalVehicle> IFatturaFacileDbContext.RentalVehicles => RentalVehicles;

IQueryable<RentalBooking> IFatturaFacileDbContext.RentalBookings => RentalBookings;

IQueryable<DamageAppraisal> IFatturaFacileDbContext.DamageAppraisals => DamageAppraisals;

IQueryable<ConstructionSite> IFatturaFacileDbContext.ConstructionSites => ConstructionSites;

IQueryable<BillOfQuantitiesItem> IFatturaFacileDbContext.BillOfQuantitiesItems => BillOfQuantitiesItems;

IQueryable<ProgressBillingSal> IFatturaFacileDbContext.ProgressBillingSals => ProgressBillingSals;

IQueryable<DeclarationOfConformity> IFatturaFacileDbContext.DeclarationsOfConformity => DeclarationsOfConformity;

IQueryable<ClientMatter> IFatturaFacileDbContext.ClientMatters => ClientMatters;

IQueryable<BillableTimeEntry> IFatturaFacileDbContext.BillableTimeEntries => BillableTimeEntries;

IQueryable<ProfessionalFeeNote> IFatturaFacileDbContext.ProfessionalFeeNotes => ProfessionalFeeNotes;

IQueryable<BillOfMaterials> IFatturaFacileDbContext.BillOfMaterials => BillOfMaterials;

IQueryable<BomLine> IFatturaFacileDbContext.BomLines => BomLines;

IQueryable<WorkCenter> IFatturaFacileDbContext.WorkCenters => WorkCenters;

IQueryable<ProductionOrder> IFatturaFacileDbContext.ProductionOrders => ProductionOrders;

IQueryable<PosTerminalSession> IFatturaFacileDbContext.PosTerminalSessions => PosTerminalSessions;

IQueryable<RetailReceipt> IFatturaFacileDbContext.RetailReceipts => RetailReceipts;

IQueryable<TimeEntry> IFatturaFacileDbContext.TimeEntries => TimeEntries;

IQueryable<AbsenceRecord> IFatturaFacileDbContext.AbsenceRecords => AbsenceRecords;

IQueryable<Employee> IFatturaFacileDbContext.Employees => Employees;

IQueryable<PayrollSlipSummary> IFatturaFacileDbContext.PayrollSlipSummaries => PayrollSlipSummaries;

IQueryable<GeneralLedgerAccount> IFatturaFacileDbContext.GeneralLedgerAccounts => GeneralLedgerAccounts;

IQueryable<GeneralLedgerEntry> IFatturaFacileDbContext.GeneralLedgerEntries => GeneralLedgerEntries;

IQueryable<GeneralLedgerLine> IFatturaFacileDbContext.GeneralLedgerLines => GeneralLedgerLines;

IQueryable<VatSettlementPeriod> IFatturaFacileDbContext.VatSettlementPeriods => VatSettlementPeriods;

IQueryable<ExecutiveKpiSummary> IFatturaFacileDbContext.ExecutiveKpiSummaries => ExecutiveKpiSummaries;

IQueryable<TaxDeadlineAlert> IFatturaFacileDbContext.TaxDeadlineAlerts => TaxDeadlineAlerts;

IQueryable<Tenant> IFatturaFacileDbContext.Tenants => Tenants;

IQueryable<Branch> IFatturaFacileDbContext.Branches => Branches;

IQueryable<User> IFatturaFacileDbContext.Users => Users;

IQueryable<ExchangeRate> IFatturaFacileDbContext.ExchangeRates => ExchangeRates;

IQueryable<AppTranslation> IFatturaFacileDbContext.AppTranslations => AppTranslations;

/// <summary>
/// On Model Creating.
/// </summary>
/// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Multi-Tenant Global Query Filters to all ITenantEntity types
        foreach (var clrType in modelBuilder.Model.GetEntityTypes().Select(e => e.ClrType).Where(typeof(ITenantEntity).IsAssignableFrom))
        {
            var parameter = Expression.Parameter(clrType, "e");
            var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
            var tenantIdProperty = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
            var filter = Expression.Lambda(Expression.Equal(property, tenantIdProperty), parameter);

            _ = modelBuilder.Entity(clrType).HasQueryFilter(filter);
        }

        // Conversions for Value Objects
        _ = modelBuilder.Entity<Client>(b =>
        {
            _ = b.OwnsOne(c => c.Address);
            _ = b.Property(c => c.FiscalCode).HasConversion(v => v != null ? v.Value : null, v => v != null ? new Domain.ValueObjects.FiscalCode(v) : null);
            _ = b.Property(c => c.VatNumber).HasConversion(v => v != null ? v.Value : null, v => v != null ? new Domain.ValueObjects.VatNumber(v, "IT") : null);
            _ = b.Property(c => c.QrIban).HasConversion(v => v != null ? v.Value : null, v => v != null ? new Domain.ValueObjects.QrIban(v) : null);
            _ = b.OwnsOne(c => c.CreditLimit);
        });

        _ = modelBuilder.Entity<Supplier>(b =>
        {
            _ = b.OwnsOne(s => s.Address);
            _ = b.Property(s => s.VatNumber).HasConversion(v => v != null ? v.Value : null, v => v != null ? new Domain.ValueObjects.VatNumber(v, "IT") : null);
        });

        _ = modelBuilder.Entity<ItalianElectronicInvoice>(b =>
        {
            _ = b.OwnsOne(i => i.CedenteIndirizzo);
            _ = b.OwnsOne(i => i.CessionarioIndirizzo);
        });

        _ = modelBuilder.Entity<SwissQrBill>(b =>
        {
            _ = b.OwnsOne(s => s.CreditorAddress);
            _ = b.OwnsOne(s => s.DebtorAddress);
            _ = b.OwnsOne(s => s.Amount);
            _ = b.Property(s => s.CreditorQrIban).HasConversion(v => v.Value, v => new Domain.ValueObjects.QrIban(v));
        });

        _ = modelBuilder.Entity<TransportDocument>(b => _ = b.OwnsOne(t => t.DestinationAddress));

        _ = modelBuilder.Entity<Warehouse>(b => _ = b.OwnsOne(w => w.Address));

        _ = modelBuilder.Entity<ConstructionSite>(b =>
        {
            _ = b.OwnsOne(c => c.SiteAddress);
            _ = b.OwnsOne(c => c.TotalContractBudget);
        });

        _ = modelBuilder.Entity<Employee>(b =>
        {
            _ = b.OwnsOne(e => e.HomeAddress);
            _ = b.Property(e => e.ItalianFiscalCode).HasConversion(v => v != null ? v.Value : null, v => v != null ? new Domain.ValueObjects.FiscalCode(v) : null);
        });

        _ = modelBuilder.Entity<ExecutiveKpiSummary>(b =>
        {
            _ = b.OwnsOne(e => e.TotalRevenueEur);
            _ = b.OwnsOne(e => e.TotalRevenueChf);
            _ = b.OwnsOne(e => e.PendingReceivablesEur);
            _ = b.OwnsOne(e => e.PendingReceivablesChf);
            _ = b.OwnsOne(e => e.PendingVatLiabilityEur);
            _ = b.OwnsOne(e => e.PendingVatLiabilityChf);
        });

        _ = modelBuilder.Entity<TaxDeadlineAlert>(b => _ = b.OwnsOne(t => t.EstimatedAmount));

        _ = modelBuilder.Entity<Quote>(b => _ = b.OwnsOne(q => q.TotalAmount));

        _ = modelBuilder.Entity<PurchaseOrder>(b => _ = b.OwnsOne(p => p.TotalAmount));

        _ = modelBuilder.Entity<Iso20022PaymentInstruction>(b => _ = b.OwnsOne(p => p.Amount));

        _ = modelBuilder.Entity<RentalVehicle>(b => _ = b.OwnsOne(r => r.DailyRate));

        _ = modelBuilder.Entity<RentalBooking>(b =>
        {
            _ = b.OwnsOne(r => r.TotalRentAmount);
            _ = b.OwnsOne(r => r.DepositAmount);
        });

        _ = modelBuilder.Entity<DamageAppraisal>(b => _ = b.OwnsOne(d => d.EstimatedRepairCost));

        _ = modelBuilder.Entity<ProgressBillingSal>(b =>
        {
            _ = b.OwnsOne(p => p.TotalCertifiedAmount);
            _ = b.OwnsOne(p => p.RetentionGuaranteeAmount);
        });

        _ = modelBuilder.Entity<PosTerminalSession>(b =>
        {
            _ = b.OwnsOne(p => p.InitialCashFloat);
            _ = b.OwnsOne(p => p.ClosingCashCalculated);
            _ = b.OwnsOne(p => p.CashDiscrepancy);
        });

        _ = modelBuilder.Entity<Branch>(b => _ = b.OwnsOne(br => br.Address));
    }

/// <summary>
/// Gets the current tenant id.
/// </summary>
    public Guid CurrentTenantId => _tenantProvider.CurrentTenantId;

/// <summary>
/// Save Changes Async.
/// </summary>
/// <param name="cancellationToken">The cancellation token.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var now = DateTime.UtcNow;
        var currentUser = _currentUserService.Username ?? "System";

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedBy = currentUser;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = now;
                entry.Entity.UpdatedBy = currentUser;
            }
        }

        // Automatically assign tenant ID for newly added tenant entities if missing
        if (_tenantProvider.HasTenant)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = _tenantProvider.CurrentTenantId;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
