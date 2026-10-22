// <copyright file="FatturaFacileDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

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
/// Primary Entity Framework Core database context for Fattura Facile ERP,
/// implementing cross-module DbSets, multi-tenant global query filters, and auditing hooks.
/// </summary>
/// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
/// <param name="tenantProvider">The tenant provider supplying the active tenant boundary.</param>
/// <param name="currentUserService">The current user service providing auditing actor information.</param>
public sealed class FatturaFacileDbContext(
    DbContextOptions<FatturaFacileDbContext> options,
    ITenantProvider tenantProvider,
    ICurrentUserService currentUserService) : DbContext(options: options), IFatturaFacileDbContext, IUnitOfWork
{

    /// <summary>
    /// The tenant provider used to partition database queries and operations.
    /// </summary>
    private readonly ITenantProvider tenantProvider = tenantProvider;

    /// <summary>
    /// The current user service used for audit tracking.
    /// </summary>
    private readonly ICurrentUserService currentUserService = currentUserService;

    /// <summary>
    /// Gets the database set for CRM customer accounts and cross-border billing profiles.
    /// </summary>
    public DbSet<Client> Clients => Set<Client>();

    /// <summary>
    /// Gets the database set for client contact persons and correspondence details.
    /// </summary>
    public DbSet<Contact> Contacts => Set<Contact>();

    /// <summary>
    /// Gets the database set for vendors and procurement partners.
    /// </summary>
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    /// <summary>
    /// Gets the database set for Italian SDI electronic sales invoices.
    /// </summary>
    public DbSet<ItalianElectronicInvoice> ItalianElectronicInvoices => Set<ItalianElectronicInvoice>();

    /// <summary>
    /// Gets the database set for line items associated with Italian electronic invoices.
    /// </summary>
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

    /// <summary>
    /// Gets the database set for Swiss QR-bill payment slips and transactions.
    /// </summary>
    public DbSet<SwissQrBill> SwissQrBills => Set<SwissQrBill>();

    /// <summary>
    /// Gets the database set for customer sales orders.
    /// </summary>
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    /// <summary>
    /// Gets the database set for customer sales quotations and price estimates.
    /// </summary>
    public DbSet<Quote> Quotes => Set<Quote>();

    /// <summary>
    /// Gets the database set for commercial price lists and tier pricing.
    /// </summary>
    public DbSet<PriceList> PriceLists => Set<PriceList>();

    /// <summary>
    /// Gets the database set for purchase orders issued to suppliers.
    /// </summary>
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    /// <summary>
    /// Gets the database set for inbound supplier (passive) invoices.
    /// </summary>
    public DbSet<PassiveInvoice> PassiveInvoices => Set<PassiveInvoice>();

    /// <summary>
    /// Gets the database set for ISO 20022 pain.001 payment transfer instructions.
    /// </summary>
    public DbSet<Iso20022PaymentInstruction> Iso20022PaymentInstructions => Set<Iso20022PaymentInstruction>();

    /// <summary>
    /// Gets the database set for imported bank account statements (camt.053).
    /// </summary>
    public DbSet<BankStatement> BankStatements => Set<BankStatement>();

    /// <summary>
    /// Gets the database set for individual transaction lines on bank statements.
    /// </summary>
    public DbSet<BankTransactionLine> BankTransactionLines => Set<BankTransactionLine>();

    /// <summary>
    /// Gets the database set for inventory products, materials, and service items.
    /// </summary>
    public DbSet<ItemCatalog> ItemCatalogs => Set<ItemCatalog>();

    /// <summary>
    /// Gets the database set for inventory storage locations and warehouses.
    /// </summary>
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    /// <summary>
    /// Gets the database set for stock movement entries and inventory adjustments.
    /// </summary>
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    /// <summary>
    /// Gets the database set for Italian transport shipping documents (DDT).
    /// </summary>
    public DbSet<TransportDocument> TransportDocuments => Set<TransportDocument>();

    /// <summary>
    /// Gets the database set for customer technical support tickets.
    /// </summary>
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();

    /// <summary>
    /// Gets the database set for field technician work orders and interventions.
    /// </summary>
    public DbSet<TechnicianWorkOrder> TechnicianWorkOrders => Set<TechnicianWorkOrder>();

    /// <summary>
    /// Gets the database set for customer vehicles serviced in automotive operations.
    /// </summary>
    public DbSet<CustomerVehicle> CustomerVehicles => Set<CustomerVehicle>();

    /// <summary>
    /// Gets the database set for seasonal tire hotel storage records and tread inspections.
    /// </summary>
    public DbSet<TireHotelRecord> TireHotelRecords => Set<TireHotelRecord>();

    /// <summary>
    /// Gets the database set for mandatory statutory vehicle inspection records (revisioni/MFK).
    /// </summary>
    public DbSet<StatutoryInspectionRecord> StatutoryInspectionRecords => Set<StatutoryInspectionRecord>();

    /// <summary>
    /// Gets the database set for fleet rental vehicles.
    /// </summary>
    public DbSet<RentalVehicle> RentalVehicles => Set<RentalVehicle>();

    /// <summary>
    /// Gets the database set for customer vehicle rental bookings and agreements.
    /// </summary>
    public DbSet<RentalBooking> RentalBookings => Set<RentalBooking>();

    /// <summary>
    /// Gets the database set for vehicle return damage inspection reports and repair estimates.
    /// </summary>
    public DbSet<DamageAppraisal> DamageAppraisals => Set<DamageAppraisal>();

    /// <summary>
    /// Gets the database set for construction site projects and contracts.
    /// </summary>
    public DbSet<ConstructionSite> ConstructionSites => Set<ConstructionSite>();

    /// <summary>
    /// Gets the database set for bill of quantities (computo metrico) line items.
    /// </summary>
    public DbSet<BillOfQuantitiesItem> BillOfQuantitiesItems => Set<BillOfQuantitiesItem>();

    /// <summary>
    /// Gets the database set for progress billing certificates (stato avanzamento lavori - SAL).
    /// </summary>
    public DbSet<ProgressBillingSal> ProgressBillingSals => Set<ProgressBillingSal>();

    /// <summary>
    /// Gets the database set for technical declarations of conformity (DiCo / DM 37/08).
    /// </summary>
    public DbSet<DeclarationOfConformity> DeclarationsOfConformity => Set<DeclarationOfConformity>();

    /// <summary>
    /// Gets the database set for professional advisory client files and legal/tax matters.
    /// </summary>
    public DbSet<ClientMatter> ClientMatters => Set<ClientMatter>();

    /// <summary>
    /// Gets the database set for professional advisory billable time recordings.
    /// </summary>
    public DbSet<BillableTimeEntry> BillableTimeEntries => Set<BillableTimeEntry>();

    /// <summary>
    /// Gets the database set for professional fee notes (parcelle / pro-forma avvocati/commercialisti).
    /// </summary>
    public DbSet<ProfessionalFeeNote> ProfessionalFeeNotes => Set<ProfessionalFeeNote>();

    /// <summary>
    /// Gets the database set for manufacturing bills of materials (distinte base).
    /// </summary>
    public DbSet<BillOfMaterials> BillOfMaterials => Set<BillOfMaterials>();

    /// <summary>
    /// Gets the database set for component lines within bills of materials.
    /// </summary>
    public DbSet<BomLine> BomLines => Set<BomLine>();

    /// <summary>
    /// Gets the database set for manufacturing machines, work cells, and production centers.
    /// </summary>
    public DbSet<WorkCenter> WorkCenters => Set<WorkCenter>();

    /// <summary>
    /// Gets the database set for manufacturing production orders and batches.
    /// </summary>
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

    /// <summary>
    /// Gets the database set for retail point-of-sale cash register sessions.
    /// </summary>
    public DbSet<PosTerminalSession> PosTerminalSessions => Set<PosTerminalSession>();

    /// <summary>
    /// Gets the database set for retail sales receipts (scontrini telematici).
    /// </summary>
    public DbSet<RetailReceipt> RetailReceipts => Set<RetailReceipt>();

    /// <summary>
    /// Gets the database set for employee time clock recordings (timbrature).
    /// </summary>
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

    /// <summary>
    /// Gets the database set for employee vacation, leave, and sick absence requests.
    /// </summary>
    public DbSet<AbsenceRecord> AbsenceRecords => Set<AbsenceRecord>();

    /// <summary>
    /// Gets the database set for employee human resource master data.
    /// </summary>
    public DbSet<Employee> Employees => Set<Employee>();

    /// <summary>
    /// Gets the database set for monthly payroll slip calculation summaries (buste paga).
    /// </summary>
    public DbSet<PayrollSlipSummary> PayrollSlipSummaries => Set<PayrollSlipSummary>();

    /// <summary>
    /// Gets the database set for chart of accounts general ledger accounts.
    /// </summary>
    public DbSet<GeneralLedgerAccount> GeneralLedgerAccounts => Set<GeneralLedgerAccount>();

    /// <summary>
    /// Gets the database set for double-entry journal transactions (prima nota contabile).
    /// </summary>
    public DbSet<GeneralLedgerEntry> GeneralLedgerEntries => Set<GeneralLedgerEntry>();

    /// <summary>
    /// Gets the database set for debit and credit line items within general ledger entries.
    /// </summary>
    public DbSet<GeneralLedgerLine> GeneralLedgerLines => Set<GeneralLedgerLine>();

    /// <summary>
    /// Gets the database set for periodic VAT tax settlement calculations (liquidazione IVA).
    /// </summary>
    public DbSet<VatSettlementPeriod> VatSettlementPeriods => Set<VatSettlementPeriod>();

    /// <summary>
    /// Gets the database set for multi-currency cross-border executive KPI summary snapshots.
    /// </summary>
    public DbSet<ExecutiveKpiSummary> ExecutiveKpiSummaries => Set<ExecutiveKpiSummary>();

    /// <summary>
    /// Gets the database set for Italian and Swiss statutory tax deadline compliance reminders.
    /// </summary>
    public DbSet<TaxDeadlineAlert> TaxDeadlineAlerts => Set<TaxDeadlineAlert>();

    /// <summary>
    /// Gets the database set for multi-tenant company configurations.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>
    /// Gets the database set for operating company branches and warehouse locations.
    /// </summary>
    public DbSet<Branch> Branches => Set<Branch>();

    /// <summary>
    /// Gets the database set for system application user accounts.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Gets the database set for foreign currency exchange rates (EUR/CHF).
    /// </summary>
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();

    /// <summary>
    /// Gets the database set for localized UI translation text entries.
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

    IQueryable<Iso20022PaymentInstruction> IFatturaFacileDbContext.Iso20022PaymentInstructions =>
        Iso20022PaymentInstructions;

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

    IQueryable<StatutoryInspectionRecord> IFatturaFacileDbContext.StatutoryInspectionRecords =>
        StatutoryInspectionRecords;

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
    /// Gets the tenant identifier currently active in this database context session.
    /// </summary>
    public Guid CurrentTenantId => tenantProvider.CurrentTenantId;

    /// <summary>
    /// Saves all changes made in this context to the underlying SQLite database,
    /// populating audit timestamps (created/modified) and associating new tenant-partitioned entities with the active tenant.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries     = ChangeTracker.Entries<IAuditableEntity>();
        var now         = DateTime.UtcNow;
        var currentUser = currentUserService.Username ?? "System";

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedBy    = currentUser;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = now;
                entry.Entity.UpdatedBy    = currentUser;
            }
        }

        // Automatically assign tenant ID for newly added tenant entities if missing
        if (tenantProvider.HasTenant)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = tenantProvider.CurrentTenantId;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Configures entity mappings, value object conversions (FiscalCode, VatNumber, QrIban, Money),
    /// and multi-tenant global query filters applied across all <see cref="ITenantEntity"/> instances.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder: modelBuilder);

        // Apply Multi-Tenant Global Query Filters to all ITenantEntity types
        foreach (var clrType in modelBuilder.Model.GetEntityTypes()
                                            .Select(selector: e => e.ClrType)
                                            .Where(predicate: typeof(ITenantEntity).IsAssignableFrom))
        {
            var parameter = Expression.Parameter(
                                                 type: clrType,
                                                 name: "e");

            var property = Expression.Property(
                                               expression: parameter,
                                               propertyName: nameof(ITenantEntity.TenantId));

            var tenantIdProperty = Expression.Property(
                                                       expression: Expression.Constant(value: this),
                                                       propertyName: nameof(CurrentTenantId));

            var filter = Expression.Lambda(
                                           body: Expression.Equal(
                                                                  left: property,
                                                                  right: tenantIdProperty),
                                           parameters: parameter);

            _ = modelBuilder.Entity(type: clrType).HasQueryFilter(filter: filter);
        }

        // Conversions for Value Objects
        _ = modelBuilder.Entity<Client>(buildAction: b =>
                                                     {
                                                         _ = b.OwnsOne(navigationExpression: c => c.Address);

                                                         _ = b.Property(propertyExpression: c => c.FiscalCode)
                                                              .HasConversion(
                                                                             convertToProviderExpression: v => v != null ? v.Value : null,
                                                                             convertFromProviderExpression: v => v != null
                                                                                                                     ? new Domain.ValueObjects.FiscalCode(v)
                                                                                                                     : null);

                                                         _ = b.Property(propertyExpression: c => c.VatNumber)
                                                              .HasConversion(
                                                                             convertToProviderExpression: v => v != null ? v.Value : null,
                                                                             convertFromProviderExpression: v => v != null
                                                                                                                     ? new Domain.ValueObjects.VatNumber(
                                                                                                                                                         v,
                                                                                                                                                         "IT")
                                                                                                                     : null);

                                                         _ = b.Property(propertyExpression: c => c.QrIban)
                                                              .HasConversion(
                                                                             convertToProviderExpression: v => v != null ? v.Value : null,
                                                                             convertFromProviderExpression: v => v != null
                                                                                                                     ? new Domain.ValueObjects.QrIban(v)
                                                                                                                     : null);

                                                         _ = b.OwnsOne(navigationExpression: c => c.CreditLimit);
                                                     });

        _ = modelBuilder.Entity<Supplier>(buildAction: b =>
                                                       {
                                                           _ = b.OwnsOne(navigationExpression: s => s.Address);

                                                           _ = b.Property(propertyExpression: s => s.VatNumber)
                                                                .HasConversion(
                                                                               convertToProviderExpression: v => v != null ? v.Value : null,
                                                                               convertFromProviderExpression: v => v != null
                                                                                                                       ? new Domain.ValueObjects.VatNumber(
                                                                                                                                                           v,
                                                                                                                                                           "IT")
                                                                                                                       : null);
                                                       });

        _ = modelBuilder.Entity<ItalianElectronicInvoice>(buildAction: b =>
                                                                       {
                                                                           _ = b.OwnsOne(navigationExpression: i => i.CedenteIndirizzo);
                                                                           _ = b.OwnsOne(navigationExpression: i => i.CessionarioIndirizzo);
                                                                       });

        _ = modelBuilder.Entity<SwissQrBill>(buildAction: b =>
                                                          {
                                                              _ = b.OwnsOne(navigationExpression: s => s.CreditorAddress);
                                                              _ = b.OwnsOne(navigationExpression: s => s.DebtorAddress);
                                                              _ = b.OwnsOne(navigationExpression: s => s.Amount);

                                                              _ = b.Property(propertyExpression: s => s.CreditorQrIban)
                                                                   .HasConversion(
                                                                                  convertToProviderExpression: v => v.Value,
                                                                                  convertFromProviderExpression: v => new Domain.ValueObjects.QrIban(v));
                                                          });

        _ = modelBuilder.Entity<TransportDocument>(buildAction: b => _ = b.OwnsOne(navigationExpression: t => t.DestinationAddress));

        _ = modelBuilder.Entity<Warehouse>(buildAction: b => _ = b.OwnsOne(navigationExpression: w => w.Address));

        _ = modelBuilder.Entity<ConstructionSite>(buildAction: b =>
                                                               {
                                                                   _ = b.OwnsOne(navigationExpression: c => c.SiteAddress);
                                                                   _ = b.OwnsOne(navigationExpression: c => c.TotalContractBudget);
                                                               });

        _ = modelBuilder.Entity<Employee>(buildAction: b =>
                                                       {
                                                           _ = b.OwnsOne(navigationExpression: e => e.HomeAddress);

                                                           _ = b.Property(propertyExpression: e => e.ItalianFiscalCode)
                                                                .HasConversion(
                                                                               convertToProviderExpression: v => v != null ? v.Value : null,
                                                                               convertFromProviderExpression: v => v != null
                                                                                                                       ? new Domain.ValueObjects.FiscalCode(v)
                                                                                                                       : null);
                                                       });

        _ = modelBuilder.Entity<ExecutiveKpiSummary>(buildAction: b =>
                                                                  {
                                                                      _ = b.OwnsOne(navigationExpression: e => e.TotalRevenueEur);
                                                                      _ = b.OwnsOne(navigationExpression: e => e.TotalRevenueChf);
                                                                      _ = b.OwnsOne(navigationExpression: e => e.PendingReceivablesEur);
                                                                      _ = b.OwnsOne(navigationExpression: e => e.PendingReceivablesChf);
                                                                      _ = b.OwnsOne(navigationExpression: e => e.PendingVatLiabilityEur);
                                                                      _ = b.OwnsOne(navigationExpression: e => e.PendingVatLiabilityChf);
                                                                  });

        _ = modelBuilder.Entity<TaxDeadlineAlert>(buildAction: b => _ = b.OwnsOne(navigationExpression: t => t.EstimatedAmount));

        _ = modelBuilder.Entity<Quote>(buildAction: b => _ = b.OwnsOne(navigationExpression: q => q.TotalAmount));

        _ = modelBuilder.Entity<PurchaseOrder>(buildAction: b => _ = b.OwnsOne(navigationExpression: p => p.TotalAmount));

        _ = modelBuilder.Entity<Iso20022PaymentInstruction>(buildAction: b => _ = b.OwnsOne(navigationExpression: p => p.Amount));

        _ = modelBuilder.Entity<RentalVehicle>(buildAction: b => _ = b.OwnsOne(navigationExpression: r => r.DailyRate));

        _ = modelBuilder.Entity<RentalBooking>(buildAction: b =>
                                                            {
                                                                _ = b.OwnsOne(navigationExpression: r => r.TotalRentAmount);
                                                                _ = b.OwnsOne(navigationExpression: r => r.DepositAmount);
                                                            });

        _ = modelBuilder.Entity<DamageAppraisal>(buildAction: b => _ = b.OwnsOne(navigationExpression: d => d.EstimatedRepairCost));

        _ = modelBuilder.Entity<ProgressBillingSal>(buildAction: b =>
                                                                 {
                                                                     _ = b.OwnsOne(navigationExpression: p => p.TotalCertifiedAmount);
                                                                     _ = b.OwnsOne(navigationExpression: p => p.RetentionGuaranteeAmount);
                                                                 });

        _ = modelBuilder.Entity<PosTerminalSession>(buildAction: b =>
                                                                 {
                                                                     _ = b.OwnsOne(navigationExpression: p => p.InitialCashFloat);
                                                                     _ = b.OwnsOne(navigationExpression: p => p.ClosingCashCalculated);
                                                                     _ = b.OwnsOne(navigationExpression: p => p.CashDiscrepancy);
                                                                 });

        _ = modelBuilder.Entity<Branch>(buildAction: b => _ = b.OwnsOne(navigationExpression: br => br.Address));
    }

}
