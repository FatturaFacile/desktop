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

public sealed class FatturaFacileDbContext(
    DbContextOptions<FatturaFacileDbContext> options,
    ITenantProvider tenantProvider,
    ICurrentUserService currentUserService) : DbContext(options), IFatturaFacileDbContext, IUnitOfWork
{
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    /// <summary>
    /// Crm
    /// </summary>
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    /// <summary>
    /// Sales
    /// </summary>
    public DbSet<ItalianElectronicInvoice> ItalianElectronicInvoices => Set<ItalianElectronicInvoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<SwissQrBill> SwissQrBills => Set<SwissQrBill>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();

    /// <summary>
    /// Purchasing
    /// </summary>
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PassiveInvoice> PassiveInvoices => Set<PassiveInvoice>();
    public DbSet<Iso20022PaymentInstruction> Iso20022PaymentInstructions => Set<Iso20022PaymentInstruction>();

    /// <summary>
    /// Treasury
    /// </summary>
    public DbSet<BankStatement> BankStatements => Set<BankStatement>();
    public DbSet<BankTransactionLine> BankTransactionLines => Set<BankTransactionLine>();

    /// <summary>
    /// Inventory
    /// </summary>
    public DbSet<ItemCatalog> ItemCatalogs => Set<ItemCatalog>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<TransportDocument> TransportDocuments => Set<TransportDocument>();

    /// <summary>
    /// Service Tickets
    /// </summary>
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TechnicianWorkOrder> TechnicianWorkOrders => Set<TechnicianWorkOrder>();

    /// <summary>
    /// Automotive
    /// </summary>
    public DbSet<CustomerVehicle> CustomerVehicles => Set<CustomerVehicle>();
    public DbSet<TireHotelRecord> TireHotelRecords => Set<TireHotelRecord>();
    public DbSet<StatutoryInspectionRecord> StatutoryInspectionRecords => Set<StatutoryInspectionRecord>();

    /// <summary>
    /// Fleet Rental
    /// </summary>
    public DbSet<RentalVehicle> RentalVehicles => Set<RentalVehicle>();
    public DbSet<RentalBooking> RentalBookings => Set<RentalBooking>();
    public DbSet<DamageAppraisal> DamageAppraisals => Set<DamageAppraisal>();

    /// <summary>
    /// Construction
    /// </summary>
    public DbSet<ConstructionSite> ConstructionSites => Set<ConstructionSite>();
    public DbSet<BillOfQuantitiesItem> BillOfQuantitiesItems => Set<BillOfQuantitiesItem>();
    public DbSet<ProgressBillingSal> ProgressBillingSals => Set<ProgressBillingSal>();
    public DbSet<DeclarationOfConformity> DeclarationsOfConformity => Set<DeclarationOfConformity>();

    /// <summary>
    /// Advisory
    /// </summary>
    public DbSet<ClientMatter> ClientMatters => Set<ClientMatter>();
    public DbSet<BillableTimeEntry> BillableTimeEntries => Set<BillableTimeEntry>();
    public DbSet<ProfessionalFeeNote> ProfessionalFeeNotes => Set<ProfessionalFeeNote>();

    /// <summary>
    /// Manufacturing
    /// </summary>
    public DbSet<BillOfMaterials> BillOfMaterials => Set<BillOfMaterials>();
    public DbSet<BomLine> BomLines => Set<BomLine>();
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    /// <summary>
    /// Retail Pos
    /// </summary>
    public DbSet<PosTerminalSession> PosTerminalSessions => Set<PosTerminalSession>();
    public DbSet<RetailReceipt> RetailReceipts => Set<RetailReceipt>();

    /// <summary>
    /// Time Tracking
    /// </summary>
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<AbsenceRecord> AbsenceRecords => Set<AbsenceRecord>();

    /// <summary>
    /// Payroll & HR
    /// </summary>
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayrollSlipSummary> PayrollSlipSummaries => Set<PayrollSlipSummary>();

    /// <summary>
    /// Accounting
    /// </summary>
    public DbSet<GeneralLedgerAccount> GeneralLedgerAccounts => Set<GeneralLedgerAccount>();
    public DbSet<GeneralLedgerEntry> GeneralLedgerEntries => Set<GeneralLedgerEntry>();
    public DbSet<GeneralLedgerLine> GeneralLedgerLines => Set<GeneralLedgerLine>();
    public DbSet<VatSettlementPeriod> VatSettlementPeriods => Set<VatSettlementPeriod>();

    /// <summary>
    /// Dashboard & Admin
    /// </summary>
    public DbSet<ExecutiveKpiSummary> ExecutiveKpiSummaries => Set<ExecutiveKpiSummary>();
    public DbSet<TaxDeadlineAlert> TaxDeadlineAlerts => Set<TaxDeadlineAlert>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<AppTranslation> AppTranslations => Set<AppTranslation>();

    /// <summary>
    /// Queryable interface implementations
    /// </summary>
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Multi-Tenant Global Query Filters to all ITenantEntity types
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                var tenantIdProperty = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
                var filter = Expression.Lambda(Expression.Equal(property, tenantIdProperty), parameter);

                _ = modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
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

    public Guid CurrentTenantId => _tenantProvider.CurrentTenantId;

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
