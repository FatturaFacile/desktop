// <copyright file="FatturaFacileDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// EF Core SQLite Database Context for FatturaFacile offline-first ERP monolith.
/// </summary>
public class FatturaFacileDbContext : DbContext
{
    private readonly ICurrentTenantService _currentTenantService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FatturaFacileDbContext"/> class.
    /// </summary>
    /// <param name="options">DbContext options.</param>
    /// <param name="currentTenantService">Current active tenant context.</param>
    public FatturaFacileDbContext(
        DbContextOptions<FatturaFacileDbContext> options,
        ICurrentTenantService currentTenantService)
        : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    /// <summary>
    /// Gets the tenants table.
    /// </summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>
    /// Gets the contacts table.
    /// </summary>
    public DbSet<Contact> Contacts => Set<Contact>();

    /// <summary>
    /// Gets the products catalog table.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Gets the invoices table.
    /// </summary>
    public DbSet<Invoice> Invoices => Set<Invoice>();

    /// <summary>
    /// Gets the invoice items table.
    /// </summary>
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();

    /// <summary>
    /// Gets the Swiss QR-bill compliance metadata table.
    /// </summary>
    public DbSet<InvoiceQrBillData> InvoiceQrBillData => Set<InvoiceQrBillData>();

    /// <summary>
    /// Gets the Italian SDI electronic invoicing metadata table.
    /// </summary>
    public DbSet<InvoiceSdiMetadata> InvoiceSdiMetadata => Set<InvoiceSdiMetadata>();

    /// <summary>
    /// Gets the VAT codes table.
    /// </summary>
    public DbSet<VatCode> VatCodes => Set<VatCode>();

    /// <summary>
    /// Gets the chart of accounts table.
    /// </summary>
    public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();

    /// <summary>
    /// Gets the General Ledger entries table (Prima Nota).
    /// </summary>
    public DbSet<GeneralLedgerEntry> GeneralLedgerEntries => Set<GeneralLedgerEntry>();

    /// <summary>
    /// Gets the General Ledger lines table.
    /// </summary>
    public DbSet<GeneralLedgerLine> GeneralLedgerLines => Set<GeneralLedgerLine>();

    /// <summary>
    /// Gets the HR employees table.
    /// </summary>
    public DbSet<HrEmployee> HrEmployees => Set<HrEmployee>();

    /// <summary>
    /// Gets the warehouses table.
    /// </summary>
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    /// <summary>
    /// Gets the stock items table.
    /// </summary>
    public DbSet<StockItem> StockItems => Set<StockItem>();

    /// <summary>
    /// Gets the payment methods table.
    /// </summary>
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        ConfigureCoreEntities(modelBuilder);
        ConfigureInvoicingEntities(modelBuilder);
        ConfigureAccountingEntities(modelBuilder);
        ConfigureInventoryAndHrEntities(modelBuilder);
    }

    private void ConfigureCoreEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(b =>
        {
            b.ToTable("tenants");
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(t => t.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            b.Property(t => t.LegalName).HasColumnName("legal_name").IsRequired().HasMaxLength(255);
            b.Property(t => t.FiscalCode).HasColumnName("fiscal_code").HasMaxLength(255);
            b.Property(t => t.VatNumber).HasColumnName("vat_number").HasMaxLength(255);
            b.Property(t => t.UidNumber).HasColumnName("uid_number").HasMaxLength(255);
            b.Property(t => t.Country).HasColumnName("country").IsRequired().HasMaxLength(255);
            b.Property(t => t.DefaultCurrency).HasColumnName("default_currency").IsRequired().HasMaxLength(255);
            b.Property(t => t.Iban).HasColumnName("iban").HasMaxLength(255);
            b.Property(t => t.QrIban).HasColumnName("qr_iban").HasMaxLength(255);
            b.Property(t => t.SdiCode).HasColumnName("sdi_code").HasMaxLength(255);
            b.Property(t => t.PecEmail).HasColumnName("pec_email").HasMaxLength(255);
            b.Property(t => t.Settings).HasColumnName("settings");
            b.Property(t => t.IsMaintenance).HasColumnName("is_maintenance");
            b.Property(t => t.CreatedAt).HasColumnName("created_at");
            b.Property(t => t.UpdatedAt).HasColumnName("updated_at");
            b.Property(t => t.DeletedAt).HasColumnName("deleted_at");
        });

        modelBuilder.Entity<Contact>(b =>
        {
            b.ToTable("contacts");
            b.HasKey(c => c.Id);
            b.Property(c => c.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(c => c.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            b.Property(c => c.Type).HasColumnName("type").IsRequired().HasMaxLength(255);
            b.Property(c => c.Country).HasColumnName("country").IsRequired().HasMaxLength(255);
            b.Property(c => c.CompanyName).HasColumnName("company_name").HasMaxLength(255);
            b.Property(c => c.FirstName).HasColumnName("first_name").HasMaxLength(255);
            b.Property(c => c.LastName).HasColumnName("last_name").HasMaxLength(255);
            b.Property(c => c.Email).HasColumnName("email").HasMaxLength(255);
            b.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(255);
            b.Property(c => c.VatNumber).HasColumnName("vat_number").HasMaxLength(255);
            b.Property(c => c.FiscalCode).HasColumnName("fiscal_code").HasMaxLength(255);
            b.Property(c => c.UidNumber).HasColumnName("uid_number").HasMaxLength(255);
            b.Property(c => c.SdiCode).HasColumnName("sdi_code").HasMaxLength(255);
            b.Property(c => c.Pec).HasColumnName("pec").HasMaxLength(255);
            b.Property(c => c.Iban).HasColumnName("iban").HasMaxLength(255);
            b.Property(c => c.QrIban).HasColumnName("qr_iban").HasMaxLength(255);
            b.Property(c => c.PaymentTermsDays).HasColumnName("payment_terms_days");
            b.Property(c => c.CreatedAt).HasColumnName("created_at");
            b.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            b.Property(c => c.DeletedAt).HasColumnName("deleted_at");
            b.HasQueryFilter(c => c.TenantId == null || c.TenantId == _currentTenantService.TenantId);
        });

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("products");
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(p => p.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            b.Property(p => p.Sku).HasColumnName("sku").IsRequired().HasMaxLength(255);
            b.Property(p => p.Barcode).HasColumnName("barcode").HasMaxLength(255);
            b.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            b.Property(p => p.Description).HasColumnName("description");
            b.Property(p => p.UnitMeasure).HasColumnName("unit_measure").IsRequired().HasMaxLength(255);
            b.Property(p => p.CostPrice).HasColumnName("cost_price");
            b.Property(p => p.SalePrice).HasColumnName("sale_price");
            b.Property(p => p.TaxRate).HasColumnName("tax_rate");
            b.Property(p => p.HsCode).HasColumnName("hs_code").HasMaxLength(255);
            b.Property(p => p.TrackInventory).HasColumnName("track_inventory");
            b.Property(p => p.IsActive).HasColumnName("is_active");
            b.Property(p => p.MinStock).HasColumnName("min_stock");
            b.Property(p => p.CreatedAt).HasColumnName("created_at");
            b.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            b.Property(p => p.DeletedAt).HasColumnName("deleted_at");
            b.HasQueryFilter(p => p.TenantId == null || p.TenantId == _currentTenantService.TenantId);
        });
    }

    private void ConfigureInvoicingEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(b =>
        {
            b.ToTable("invoices");
            b.HasKey(i => i.Id);
            b.Property(i => i.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(i => i.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            b.Property(i => i.ContactId).HasColumnName("contact_id").IsRequired().HasMaxLength(36);
            b.Property(i => i.InvoiceType).HasColumnName("invoice_type").IsRequired().HasMaxLength(255);
            b.Property(i => i.InvoiceNumber).HasColumnName("invoice_number").IsRequired().HasMaxLength(255);
            b.Property(i => i.Date).HasColumnName("date").IsRequired();
            b.Property(i => i.DueDate).HasColumnName("due_date");
            b.Property(i => i.Currency).HasColumnName("currency").IsRequired().HasMaxLength(255);
            b.Property(i => i.ExchangeRate).HasColumnName("exchange_rate");
            b.Property(i => i.Subtotal).HasColumnName("subtotal");
            b.Property(i => i.DiscountAmount).HasColumnName("discount_amount");
            b.Property(i => i.TotalNet).HasColumnName("total_net");
            b.Property(i => i.TotalTax).HasColumnName("total_tax");
            b.Property(i => i.TotalGross).HasColumnName("total_gross");
            b.Property(i => i.RoundingAmount).HasColumnName("rounding_amount");
            b.Property(i => i.TotalPaid).HasColumnName("total_paid");
            b.Property(i => i.ReceivableAmount).HasColumnName("receivable_amount");
            b.Property(i => i.PaymentStatus).HasColumnName("payment_status").IsRequired().HasMaxLength(255);
            b.Property(i => i.Jurisdiction).HasColumnName("jurisdiction").IsRequired().HasMaxLength(255);
            b.Property(i => i.Status).HasColumnName("status").IsRequired().HasMaxLength(255);
            b.Property(i => i.SplitPayment).HasColumnName("split_payment");
            b.Property(i => i.ReverseCharge).HasColumnName("reverse_charge");
            b.Property(i => i.WithholdingTaxEnabled).HasColumnName("withholding_tax_enabled");
            b.Property(i => i.WithholdingTaxPercent).HasColumnName("withholding_tax_percent");
            b.Property(i => i.WithholdingTaxAmount).HasColumnName("withholding_tax_amount");
            b.Property(i => i.StampDutyApplied).HasColumnName("stamp_duty_applied");
            b.Property(i => i.StampDutyAmount).HasColumnName("stamp_duty_amount");
            b.Property(i => i.FiscalYear).HasColumnName("fiscal_year");
            b.Property(i => i.SequentialNumber).HasColumnName("sequential_number");
            b.Property(i => i.PaymentMethod).HasColumnName("payment_method").HasMaxLength(255);
            b.Property(i => i.BankIban).HasColumnName("bank_iban").HasMaxLength(255);
            b.Property(i => i.Notes).HasColumnName("notes");
            b.Property(i => i.PostedAt).HasColumnName("posted_at");
            b.Property(i => i.CreatedAt).HasColumnName("created_at");
            b.Property(i => i.UpdatedAt).HasColumnName("updated_at");
            b.Property(i => i.DeletedAt).HasColumnName("deleted_at");

            b.HasOne(i => i.Contact).WithMany().HasForeignKey(i => i.ContactId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(i => i.Items).WithOne(it => it.Invoice).HasForeignKey(it => it.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(i => i.QrBillData).WithOne(q => q.Invoice).HasForeignKey<InvoiceQrBillData>(q => q.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(i => i.SdiMetadata).WithOne(s => s.Invoice).HasForeignKey<InvoiceSdiMetadata>(s => s.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            b.HasQueryFilter(i => i.TenantId == _currentTenantService.TenantId);
        });

        modelBuilder.Entity<InvoiceItem>(b =>
        {
            b.ToTable("invoice_items");
            b.HasKey(it => it.Id);
            b.Property(it => it.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(it => it.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            b.Property(it => it.InvoiceId).HasColumnName("invoice_id").IsRequired().HasMaxLength(36);
            b.Property(it => it.ProductId).HasColumnName("product_id").HasMaxLength(36);
            b.Property(it => it.Description).HasColumnName("description").IsRequired().HasMaxLength(255);
            b.Property(it => it.Quantity).HasColumnName("quantity");
            b.Property(it => it.UnitPrice).HasColumnName("unit_price");
            b.Property(it => it.TaxRate).HasColumnName("tax_rate");
            b.Property(it => it.DiscountPercentage).HasColumnName("discount_percentage");
            b.Property(it => it.LineTotal).HasColumnName("line_total");
            b.Property(it => it.Uom).HasColumnName("uom").IsRequired().HasMaxLength(255);
            b.Property(it => it.Notes).HasColumnName("notes");
            b.Property(it => it.CreatedAt).HasColumnName("created_at");
            b.Property(it => it.UpdatedAt).HasColumnName("updated_at");
            b.Property(it => it.DeletedAt).HasColumnName("deleted_at");
            b.HasOne(it => it.Product).WithMany().HasForeignKey(it => it.ProductId).OnDelete(DeleteBehavior.SetNull);
            b.HasQueryFilter(it => it.TenantId == _currentTenantService.TenantId);
        });

        modelBuilder.Entity<InvoiceQrBillData>(b =>
        {
            b.ToTable("invoice_qr_bill_data");
            b.HasKey(q => q.Id);
            b.Property(q => q.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(q => q.InvoiceId).HasColumnName("invoice_id").IsRequired().HasMaxLength(36);
            b.Property(q => q.QrIban).HasColumnName("qr_iban").IsRequired().HasMaxLength(255);
            b.Property(q => q.ScorReference).HasColumnName("scor_reference").IsRequired().HasMaxLength(255);
            b.Property(q => q.CreditorInfo).HasColumnName("creditor_info").IsRequired();
            b.Property(q => q.DebtorInfo).HasColumnName("debtor_info").IsRequired();
            b.Property(q => q.QrBillSvg).HasColumnName("qr_bill_svg");
            b.Property(q => q.CreatedAt).HasColumnName("created_at");
            b.Property(q => q.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<InvoiceSdiMetadata>(b =>
        {
            b.ToTable("invoice_sdi_metadata");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(s => s.InvoiceId).HasColumnName("invoice_id").IsRequired().HasMaxLength(36);
            b.Property(s => s.TransmissionFormat).HasColumnName("transmission_format").IsRequired().HasMaxLength(255);
            b.Property(s => s.SdiIdentifier).HasColumnName("sdi_identifier").HasMaxLength(255);
            b.Property(s => s.SdiStatus).HasColumnName("sdi_status").IsRequired().HasMaxLength(255);
            b.Property(s => s.FileName).HasColumnName("file_name").HasMaxLength(255);
            b.Property(s => s.XmlContent).HasColumnName("xml_content");
            b.Property(s => s.SentAt).HasColumnName("sent_at");
            b.Property(s => s.DeliveredAt).HasColumnName("delivered_at");
            b.Property(s => s.CreatedAt).HasColumnName("created_at");
            b.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        });
    }

    private void ConfigureAccountingEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VatCode>(b =>
        {
            b.ToTable("vat_codes");
            b.HasKey(v => v.Id);
            b.Property(v => v.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(v => v.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            b.Property(v => v.Code).HasColumnName("code").IsRequired().HasMaxLength(255);
            b.Property(v => v.Description).HasColumnName("description").HasMaxLength(255);
            b.Property(v => v.CountryIso).HasColumnName("country_iso").IsRequired().HasMaxLength(255);
            b.Property(v => v.Rate).HasColumnName("rate");
            b.Property(v => v.TaxRatePercent).HasColumnName("tax_rate_percent");
            b.Property(v => v.Nature).HasColumnName("nature").HasMaxLength(255);
            b.Property(v => v.IsSplitPayment).HasColumnName("is_split_payment");
            b.Property(v => v.IsReverseCharge).HasColumnName("is_reverse_charge");
            b.Property(v => v.IsActive).HasColumnName("is_active");
            b.Property(v => v.CreatedAt).HasColumnName("created_at");
            b.Property(v => v.UpdatedAt).HasColumnName("updated_at");
            b.Property(v => v.DeletedAt).HasColumnName("deleted_at");
            b.HasQueryFilter(v => v.TenantId == null || v.TenantId == _currentTenantService.TenantId);
        });

        modelBuilder.Entity<ChartOfAccount>(b =>
        {
            b.ToTable("chart_of_accounts");
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(a => a.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            b.Property(a => a.Code).HasColumnName("code").HasMaxLength(255);
            b.Property(a => a.AccountCode).HasColumnName("account_code").HasMaxLength(255);
            b.Property(a => a.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            b.Property(a => a.Type).HasColumnName("type").IsRequired().HasMaxLength(255);
            b.Property(a => a.Standard).HasColumnName("standard").IsRequired().HasMaxLength(255);
            b.Property(a => a.AccountClass).HasColumnName("account_class").IsRequired().HasMaxLength(255);
            b.Property(a => a.AccountType).HasColumnName("account_type").IsRequired().HasMaxLength(255);
            b.Property(a => a.NormalBalance).HasColumnName("normal_balance").IsRequired().HasMaxLength(255);
            b.Property(a => a.Currency).HasColumnName("currency").IsRequired().HasMaxLength(255);
            b.Property(a => a.AllowsManualPosting).HasColumnName("allows_manual_posting");
            b.Property(a => a.IsActive).HasColumnName("is_active");
            b.Property(a => a.CreatedAt).HasColumnName("created_at");
            b.Property(a => a.UpdatedAt).HasColumnName("updated_at");
            b.Property(a => a.DeletedAt).HasColumnName("deleted_at");
            b.HasQueryFilter(a => a.TenantId == null || a.TenantId == _currentTenantService.TenantId);
        });

        modelBuilder.Entity<GeneralLedgerEntry>(b =>
        {
            b.ToTable("general_ledger_entries");
            b.HasKey(g => g.Id);
            b.Property(g => g.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(g => g.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            b.Property(g => g.EntryNumber).HasColumnName("entry_number").IsRequired().HasMaxLength(255);
            b.Property(g => g.PostingDate).HasColumnName("posting_date").IsRequired();
            b.Property(g => g.DocumentDate).HasColumnName("document_date").IsRequired();
            b.Property(g => g.Description).HasColumnName("description").IsRequired();
            b.Property(g => g.SourceDocumentType).HasColumnName("source_document_type").HasMaxLength(255);
            b.Property(g => g.ReferenceId).HasColumnName("reference_id").HasMaxLength(36);
            b.Property(g => g.IsLocked).HasColumnName("is_locked");
            b.Property(g => g.CreatedAt).HasColumnName("created_at");
            b.Property(g => g.UpdatedAt).HasColumnName("updated_at");
            b.Property(g => g.DeletedAt).HasColumnName("deleted_at");

            b.HasMany(g => g.Lines).WithOne(l => l.GeneralLedgerEntry).HasForeignKey(l => l.GeneralLedgerEntryId).OnDelete(DeleteBehavior.Cascade);
            b.HasQueryFilter(g => g.TenantId == _currentTenantService.TenantId);
        });

        modelBuilder.Entity<GeneralLedgerLine>(b =>
        {
            b.ToTable("general_ledger_lines");
            b.HasKey(l => l.Id);
            b.Property(l => l.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(l => l.GeneralLedgerEntryId).HasColumnName("general_ledger_entry_id").IsRequired().HasMaxLength(36);
            b.Property(l => l.AccountId).HasColumnName("account_id").IsRequired().HasMaxLength(36);
            b.Property(l => l.LineNumber).HasColumnName("line_number");
            b.Property(l => l.Debit).HasColumnName("debit");
            b.Property(l => l.Credit).HasColumnName("credit");
            b.Property(l => l.Description).HasColumnName("description");
            b.Property(l => l.CreatedAt).HasColumnName("created_at");
            b.Property(l => l.UpdatedAt).HasColumnName("updated_at");
            b.HasOne(l => l.Account).WithMany().HasForeignKey(l => l.AccountId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureInventoryAndHrEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HrEmployee>(b =>
        {
            b.ToTable("hr_employees");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(e => e.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            b.Property(e => e.EmployeeNumber).HasColumnName("employee_number").IsRequired().HasMaxLength(255);
            b.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(255);
            b.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(255);
            b.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            b.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(255);
            b.Property(e => e.SocialSecurityNumber).HasColumnName("social_security_number").HasMaxLength(255);
            b.Property(e => e.DepartmentName).HasColumnName("department_name").HasMaxLength(255);
            b.Property(e => e.JobTitle).HasColumnName("job_title").HasMaxLength(255);
            b.Property(e => e.EmploymentStatus).HasColumnName("employment_status").IsRequired().HasMaxLength(255);
            b.Property(e => e.EmploymentType).HasColumnName("employment_type").IsRequired().HasMaxLength(255);
            b.Property(e => e.HireDate).HasColumnName("hire_date").IsRequired();
            b.Property(e => e.TerminationDate).HasColumnName("termination_date");
            b.Property(e => e.BaseAnnualSalary).HasColumnName("base_annual_salary");
            b.Property(e => e.HourlyCostRate).HasColumnName("hourly_cost_rate");
            b.Property(e => e.Currency).HasColumnName("currency").IsRequired().HasMaxLength(255);
            b.Property(e => e.FteRatio).HasColumnName("fte_ratio");
            b.Property(e => e.WorkScheduleCode).HasColumnName("work_schedule_code").IsRequired().HasMaxLength(255);
            b.Property(e => e.CreatedAt).HasColumnName("created_at");
            b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            b.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            b.HasQueryFilter(e => e.TenantId == _currentTenantService.TenantId);
        });

        modelBuilder.Entity<Warehouse>(b =>
        {
            b.ToTable("warehouses");
            b.HasKey(w => w.Id);
            b.Property(w => w.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(w => w.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            b.Property(w => w.Code).HasColumnName("code").IsRequired().HasMaxLength(255);
            b.Property(w => w.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            b.Property(w => w.Address).HasColumnName("address").HasMaxLength(255);
            b.Property(w => w.IsPrimary).HasColumnName("is_primary");
            b.Property(w => w.WarehouseType).HasColumnName("warehouse_type").IsRequired().HasMaxLength(255);
            b.Property(w => w.IsActive).HasColumnName("is_active");
            b.Property(w => w.CreatedAt).HasColumnName("created_at");
            b.Property(w => w.UpdatedAt).HasColumnName("updated_at");
            b.Property(w => w.DeletedAt).HasColumnName("deleted_at");
            b.HasQueryFilter(w => w.TenantId == null || w.TenantId == _currentTenantService.TenantId);
        });

        modelBuilder.Entity<StockItem>(b =>
        {
            b.ToTable("stock_items");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(s => s.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            b.Property(s => s.Sku).HasColumnName("sku").IsRequired().HasMaxLength(255);
            b.Property(s => s.BarcodeEan13).HasColumnName("barcode_ean13").HasMaxLength(255);
            b.Property(s => s.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            b.Property(s => s.Description).HasColumnName("description");
            b.Property(s => s.Uom).HasColumnName("uom").IsRequired().HasMaxLength(255);
            b.Property(s => s.StandardCost).HasColumnName("standard_cost");
            b.Property(s => s.CurrentMovingAverageCost).HasColumnName("current_moving_average_cost");
            b.Property(s => s.WeightKg).HasColumnName("weight_kg");
            b.Property(s => s.IsActive).HasColumnName("is_active");
            b.Property(s => s.CreatedAt).HasColumnName("created_at");
            b.Property(s => s.UpdatedAt).HasColumnName("updated_at");
            b.Property(s => s.DeletedAt).HasColumnName("deleted_at");
            b.HasQueryFilter(s => s.TenantId == _currentTenantService.TenantId);
        });

        modelBuilder.Entity<PaymentMethod>(b =>
        {
            b.ToTable("payment_methods");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).HasColumnName("id").HasMaxLength(36);
            b.Property(m => m.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            b.Property(m => m.Code).HasColumnName("code").IsRequired().HasMaxLength(255);
            b.Property(m => m.IsElectronic).HasColumnName("is_electronic");
            b.Property(m => m.RequiresIban).HasColumnName("requires_iban");
            b.Property(m => m.IsActive).HasColumnName("is_active");
            b.Property(m => m.CreatedAt).HasColumnName("created_at");
            b.Property(m => m.UpdatedAt).HasColumnName("updated_at");
            b.HasQueryFilter(m => m.TenantId == null || m.TenantId == _currentTenantService.TenantId);
        });
    }
}
