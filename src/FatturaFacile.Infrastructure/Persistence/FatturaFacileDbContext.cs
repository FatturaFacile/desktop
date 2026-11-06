// <copyright file="FatturaFacileDbContext.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FatturaFacile.Infrastructure.Persistence;

/// <summary>
/// EF Core SQLite Database Context for FatturaFacile offline-first ERP monolith.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FatturaFacileDbContext"/> class.
/// </remarks>
/// <param name="options">DbContext options.</param>
/// <param name="currentTenantService">Current active tenant context.</param>
/// <param name="fieldEncryptor">Encryptor for sensitive personal data columns (null disables column encryption, e.g. in tests).</param>
public class FatturaFacileDbContext(
    DbContextOptions<FatturaFacileDbContext> options,
    ICurrentTenantService currentTenantService,
    AesGcmFieldEncryptor? fieldEncryptor = null) : DbContext(options)
{

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

    /// <summary>
    /// Gets the application users table.
    /// </summary>
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    /// <summary>
    /// Gets the user tenant roles table.
    /// </summary>
    public DbSet<UserTenantRole> UserTenantRoles => Set<UserTenantRole>();

    /// <summary>
    /// Gets the payroll wage types.
    /// </summary>
    public DbSet<PayrollWageType> PayrollWageTypes => Set<PayrollWageType>();

    /// <summary>
    /// Gets the payroll inputs.
    /// </summary>
    public DbSet<PayrollInput> PayrollInputs => Set<PayrollInput>();

    /// <summary>
    /// Gets the hr absences.
    /// </summary>
    public DbSet<HrAbsence> HrAbsences => Set<HrAbsence>();

    /// <summary>
    /// Gets the hr employee children.
    /// </summary>
    public DbSet<HrEmployeeChild> HrEmployeeChildren => Set<HrEmployeeChild>();

    /// <summary>
    /// Gets the withholding tax tariffs.
    /// </summary>
    public DbSet<WithholdingTaxTariff> WithholdingTaxTariffs => Set<WithholdingTaxTariff>();

    /// <summary>
    /// Gets the family allowance rates.
    /// </summary>
    public DbSet<FamilyAllowanceRate> FamilyAllowanceRates => Set<FamilyAllowanceRate>();

    /// <summary>
    /// Gets the swiss payroll settings.
    /// </summary>
    public DbSet<SwissPayrollSettings> SwissPayrollSettings => Set<SwissPayrollSettings>();

    /// <summary>
    /// Gets the payroll runs.
    /// </summary>
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();

    /// <summary>
    /// Gets the payslips.
    /// </summary>
    public DbSet<Payslip> Payslips => Set<Payslip>();

    /// <summary>
    /// Gets the payslip lines.
    /// </summary>
    public DbSet<PayslipLine> PayslipLines => Set<PayslipLine>();

    /// <summary>
    /// Gets the salary certificates.
    /// </summary>
    public DbSet<SalaryCertificate> SalaryCertificates => Set<SalaryCertificate>();

    /// <summary>
    /// Gets the time clock events.
    /// </summary>
    public DbSet<TimeClockEvent> TimeClockEvents => Set<TimeClockEvent>();

    /// <summary>
    /// Gets the time daily records.
    /// </summary>
    public DbSet<TimeDailyRecord> TimeDailyRecords => Set<TimeDailyRecord>();

    /// <summary>
    /// Gets the time audit entries.
    /// </summary>
    public DbSet<TimeAuditEntry> TimeAuditEntries => Set<TimeAuditEntry>();

    /// <summary>
    /// Gets the public holidays.
    /// </summary>
    public DbSet<PublicHoliday> PublicHolidays => Set<PublicHoliday>();

    /// <summary>
    /// Gets the purchase invoices.
    /// </summary>
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();

    /// <summary>
    /// Gets the purchase invoice lines.
    /// </summary>
    public DbSet<PurchaseInvoiceLine> PurchaseInvoiceLines => Set<PurchaseInvoiceLine>();

    /// <summary>
    /// Gets the bank accounts.
    /// </summary>
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    /// <summary>
    /// Gets the bank transactions.
    /// </summary>
    public DbSet<BankTransaction> BankTransactions => Set<BankTransaction>();

    /// <summary>
    /// Gets the vat returns.
    /// </summary>
    public DbSet<VatReturn> VatReturns => Set<VatReturn>();

    /// <summary>
    /// Gets the fixed assets.
    /// </summary>
    public DbSet<FixedAsset> FixedAssets => Set<FixedAsset>();

    /// <summary>
    /// Gets the fixed asset depreciations.
    /// </summary>
    public DbSet<FixedAssetDepreciation> FixedAssetDepreciations => Set<FixedAssetDepreciation>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        base.OnModelCreating(modelBuilder);

        ConfigureCoreEntities(modelBuilder);
        ConfigureInvoicingEntities(modelBuilder);
        ConfigureAccountingEntities(modelBuilder);
        ConfigureInventoryAndHrEntities(modelBuilder);
        ConfigureSecurityEntities(modelBuilder);
        ConfigurePayrollEntities(modelBuilder);
        ConfigureFieldEncryption(modelBuilder);
    }

    private static void ConfigureSecurityEntities(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<AppUser>(b =>
        {
            _ = b.ToTable("app_users");
            _ = b.HasKey(u => u.Id);
            _ = b.Property(u => u.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(u => u.UserName).HasColumnName("user_name").IsRequired().HasMaxLength(64);
            _ = b.Property(u => u.DisplayName).HasColumnName("display_name").IsRequired().HasMaxLength(128);
            _ = b.Property(u => u.WindowsAccountName).HasColumnName("windows_account_name").HasMaxLength(256);
            _ = b.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(256);
            _ = b.Property(u => u.IsActive).HasColumnName("is_active");
            _ = b.Property(u => u.IsSystemAdministrator).HasColumnName("is_system_administrator");
            _ = b.Property(u => u.FailedSignInCount).HasColumnName("failed_sign_in_count");
            _ = b.Property(u => u.LockoutEndUtc).HasColumnName("lockout_end_utc");
            _ = b.Property(u => u.LastSignInUtc).HasColumnName("last_sign_in_utc");
            _ = b.Property(u => u.CreatedAt).HasColumnName("created_at");
            _ = b.Property(u => u.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(u => u.UserName).IsUnique();
            _ = b.HasIndex(u => u.WindowsAccountName).IsUnique();
            _ = b.HasMany(u => u.TenantRoles).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        _ = modelBuilder.Entity<UserTenantRole>(b =>
        {
            _ = b.ToTable("user_tenant_roles");
            _ = b.HasKey(r => r.Id);
            _ = b.Property(r => r.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(r => r.UserId).HasColumnName("user_id").IsRequired().HasMaxLength(36);
            _ = b.Property(r => r.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            _ = b.Property(r => r.Role).HasColumnName("role").IsRequired().HasMaxLength(32);
            _ = b.Property(r => r.CreatedAt).HasColumnName("created_at");
            _ = b.Property(r => r.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasIndex(r => new { r.UserId, r.TenantId }).IsUnique();
        });
    }

    private static void MapToSnakeCase(EntityTypeBuilder builder, string table)
    {
        _ = builder.ToTable(table);

        foreach (var property in builder.Metadata.GetProperties())
        {
            property.SetColumnName(ToSnakeCase(property.Name));
        }
    }

    private static string ToSnakeCase(string name)
    {
        StringBuilder result = new();

        for (var i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]))
            {
                _ = result.Append('_');
            }

            _ = result.Append(char.ToLowerInvariant(name[i]));
        }

        return result.ToString();
    }

    private void ConfigurePayrollEntities(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<PayrollWageType>(b =>
        {
            MapToSnakeCase(b, "payroll_wage_types");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        });

        _ = modelBuilder.Entity<PayrollInput>(b =>
        {
            MapToSnakeCase(b, "payroll_inputs");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.Year, x.Month });
        });

        _ = modelBuilder.Entity<HrAbsence>(b =>
        {
            MapToSnakeCase(b, "hr_absences");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<HrEmployeeChild>(b =>
        {
            MapToSnakeCase(b, "hr_employee_children");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<WithholdingTaxTariff>(b =>
        {
            MapToSnakeCase(b, "withholding_tax_tariffs");
            _ = b.HasIndex(x => new { x.Canton, x.TariffCode, x.ValidFrom });
        });

        _ = modelBuilder.Entity<FamilyAllowanceRate>(b => MapToSnakeCase(b, "family_allowance_rates"));

        _ = modelBuilder.Entity<SwissPayrollSettings>(b =>
        {
            MapToSnakeCase(b, "swiss_payroll_settings");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => x.TenantId).IsUnique();
        });

        _ = modelBuilder.Entity<PayrollRun>(b =>
        {
            MapToSnakeCase(b, "payroll_runs");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.Year, x.Month }).IsUnique();
        });

        _ = modelBuilder.Entity<Payslip>(b =>
        {
            MapToSnakeCase(b, "payslips");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => x.PayrollRunId);
        });

        _ = modelBuilder.Entity<PayslipLine>(b =>
        {
            MapToSnakeCase(b, "payslip_lines");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => x.PayslipId);
        });

        _ = modelBuilder.Entity<SalaryCertificate>(b =>
        {
            MapToSnakeCase(b, "salary_certificates");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.Year, x.EmployeeId }).IsUnique();
        });

        _ = modelBuilder.Entity<TimeClockEvent>(b =>
        {
            MapToSnakeCase(b, "time_clock_events");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.Timestamp });
        });

        _ = modelBuilder.Entity<TimeDailyRecord>(b =>
        {
            MapToSnakeCase(b, "time_daily_records");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.EmployeeId, x.Date }).IsUnique();
        });

        _ = modelBuilder.Entity<TimeAuditEntry>(b =>
        {
            MapToSnakeCase(b, "time_audit_entries");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.Timestamp });
        });

        _ = modelBuilder.Entity<PublicHoliday>(b =>
        {
            MapToSnakeCase(b, "public_holidays");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.Date });
        });

        _ = modelBuilder.Entity<PurchaseInvoice>(b =>
        {
            MapToSnakeCase(b, "purchase_invoices");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.SupplierId, x.DocumentNumber });
        });

        _ = modelBuilder.Entity<PurchaseInvoiceLine>(b =>
        {
            MapToSnakeCase(b, "purchase_invoice_lines");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => x.PurchaseInvoiceId);
        });

        _ = modelBuilder.Entity<BankAccount>(b =>
        {
            MapToSnakeCase(b, "bank_accounts");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.Iban });
        });

        _ = modelBuilder.Entity<BankTransaction>(b =>
        {
            MapToSnakeCase(b, "bank_transactions");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.BankAccountId, x.ExternalId });
        });

        _ = modelBuilder.Entity<VatReturn>(b =>
        {
            MapToSnakeCase(b, "vat_returns");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.PeriodFrom });
        });

        _ = modelBuilder.Entity<FixedAsset>(b =>
        {
            MapToSnakeCase(b, "fixed_assets");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.Code });
        });

        _ = modelBuilder.Entity<FixedAssetDepreciation>(b =>
        {
            MapToSnakeCase(b, "fixed_asset_depreciations");
            _ = b.HasQueryFilter(x => x.TenantId == currentTenantService.TenantId);
            _ = b.HasIndex(x => new { x.TenantId, x.FixedAssetId, x.Year }).IsUnique();
        });
    }

    private void ConfigureFieldEncryption(ModelBuilder modelBuilder)
    {
        if (fieldEncryptor is null)
        {
            return;
        }

        var encryptor = fieldEncryptor;
        ValueConverter<string?, string?> converter = new(v => encryptor.Encrypt(v), v => encryptor.Decrypt(v));

        _ = modelBuilder.Entity<HrEmployee>(b =>
        {
            _ = b.Property(e => e.SocialSecurityNumber).HasConversion(converter);
            _ = b.Property(e => e.Email).HasConversion(converter);
            _ = b.Property(e => e.Phone).HasConversion(converter);
            _ = b.Property(e => e.Iban).HasConversion(converter);
        });

        _ = modelBuilder.Entity<Contact>(b => b.Property(c => c.Iban).HasConversion(converter));
    }

    private void ConfigureCoreEntities(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Tenant>(b =>
        {
            _ = b.ToTable("tenants");
            _ = b.HasKey(t => t.Id);
            _ = b.Property(t => t.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(t => t.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            _ = b.Property(t => t.LegalName).HasColumnName("legal_name").IsRequired().HasMaxLength(255);
            _ = b.Property(t => t.FiscalCode).HasColumnName("fiscal_code").HasMaxLength(255);
            _ = b.Property(t => t.VatNumber).HasColumnName("vat_number").HasMaxLength(255);
            _ = b.Property(t => t.UidNumber).HasColumnName("uid_number").HasMaxLength(255);
            _ = b.Property(t => t.Country).HasColumnName("country").IsRequired().HasMaxLength(255);
            _ = b.Property(t => t.DefaultCurrency).HasColumnName("default_currency").IsRequired().HasMaxLength(255);
            _ = b.Property(t => t.Iban).HasColumnName("iban").HasMaxLength(255);
            _ = b.Property(t => t.QrIban).HasColumnName("qr_iban").HasMaxLength(255);
            _ = b.Property(t => t.SdiCode).HasColumnName("sdi_code").HasMaxLength(255);
            _ = b.Property(t => t.PecEmail).HasColumnName("pec_email").HasMaxLength(255);
            _ = b.Property(t => t.Settings).HasColumnName("settings");
            _ = b.Property(t => t.IsMaintenance).HasColumnName("is_maintenance");
            _ = b.Property(t => t.Street).HasColumnName("street").HasMaxLength(70);
            _ = b.Property(t => t.HouseNumber).HasColumnName("house_number").HasMaxLength(16);
            _ = b.Property(t => t.PostalCode).HasColumnName("postal_code").HasMaxLength(16);
            _ = b.Property(t => t.City).HasColumnName("city").HasMaxLength(35);
            _ = b.Property(t => t.Province).HasColumnName("province").HasMaxLength(2);
            _ = b.Property(t => t.TaxRegime).HasColumnName("tax_regime").IsRequired().HasMaxLength(4);
            _ = b.Property(t => t.CreatedAt).HasColumnName("created_at");
            _ = b.Property(t => t.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(t => t.DeletedAt).HasColumnName("deleted_at");
        });

        _ = modelBuilder.Entity<Contact>(b =>
        {
            _ = b.ToTable("contacts");
            _ = b.HasKey(c => c.Id);
            _ = b.Property(c => c.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(c => c.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(c => c.Type).HasColumnName("type").IsRequired().HasMaxLength(255);
            _ = b.Property(c => c.Country).HasColumnName("country").IsRequired().HasMaxLength(255);
            _ = b.Property(c => c.CompanyName).HasColumnName("company_name").HasMaxLength(255);
            _ = b.Property(c => c.FirstName).HasColumnName("first_name").HasMaxLength(255);
            _ = b.Property(c => c.LastName).HasColumnName("last_name").HasMaxLength(255);
            _ = b.Property(c => c.Email).HasColumnName("email").HasMaxLength(255);
            _ = b.Property(c => c.Phone).HasColumnName("phone").HasMaxLength(255);
            _ = b.Property(c => c.VatNumber).HasColumnName("vat_number").HasMaxLength(255);
            _ = b.Property(c => c.FiscalCode).HasColumnName("fiscal_code").HasMaxLength(255);
            _ = b.Property(c => c.UidNumber).HasColumnName("uid_number").HasMaxLength(255);
            _ = b.Property(c => c.SdiCode).HasColumnName("sdi_code").HasMaxLength(255);
            _ = b.Property(c => c.Pec).HasColumnName("pec").HasMaxLength(255);
            _ = b.Property(c => c.Iban).HasColumnName("iban").HasMaxLength(255);
            _ = b.Property(c => c.QrIban).HasColumnName("qr_iban").HasMaxLength(255);
            _ = b.Property(c => c.PaymentTermsDays).HasColumnName("payment_terms_days");
            _ = b.Property(c => c.Street).HasColumnName("street").HasMaxLength(70);
            _ = b.Property(c => c.HouseNumber).HasColumnName("house_number").HasMaxLength(16);
            _ = b.Property(c => c.PostalCode).HasColumnName("postal_code").HasMaxLength(16);
            _ = b.Property(c => c.City).HasColumnName("city").HasMaxLength(35);
            _ = b.Property(c => c.Province).HasColumnName("province").HasMaxLength(2);
            _ = b.Property(c => c.IsPublicAdministration).HasColumnName("is_public_administration");
            _ = b.Property(c => c.CreatedAt).HasColumnName("created_at");
            _ = b.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(c => c.DeletedAt).HasColumnName("deleted_at");
            _ = b.HasQueryFilter(c => c.TenantId == null || c.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<Product>(b =>
        {
            _ = b.ToTable("products");
            _ = b.HasKey(p => p.Id);
            _ = b.Property(p => p.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(p => p.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(p => p.Sku).HasColumnName("sku").IsRequired().HasMaxLength(255);
            _ = b.Property(p => p.Barcode).HasColumnName("barcode").HasMaxLength(255);
            _ = b.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            _ = b.Property(p => p.Description).HasColumnName("description");
            _ = b.Property(p => p.UnitMeasure).HasColumnName("unit_measure").IsRequired().HasMaxLength(255);
            _ = b.Property(p => p.CostPrice).HasColumnName("cost_price");
            _ = b.Property(p => p.SalePrice).HasColumnName("sale_price");
            _ = b.Property(p => p.TaxRate).HasColumnName("tax_rate");
            _ = b.Property(p => p.HsCode).HasColumnName("hs_code").HasMaxLength(255);
            _ = b.Property(p => p.TrackInventory).HasColumnName("track_inventory");
            _ = b.Property(p => p.IsActive).HasColumnName("is_active");
            _ = b.Property(p => p.MinStock).HasColumnName("min_stock");
            _ = b.Property(p => p.CreatedAt).HasColumnName("created_at");
            _ = b.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(p => p.DeletedAt).HasColumnName("deleted_at");
            _ = b.HasQueryFilter(p => p.TenantId == null || p.TenantId == currentTenantService.TenantId);
        });
    }

    private void ConfigureInvoicingEntities(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<Invoice>(b =>
        {
            _ = b.ToTable("invoices");
            _ = b.HasKey(i => i.Id);
            _ = b.Property(i => i.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(i => i.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            _ = b.Property(i => i.ContactId).HasColumnName("contact_id").IsRequired().HasMaxLength(36);
            _ = b.Property(i => i.InvoiceType).HasColumnName("invoice_type").IsRequired().HasMaxLength(255);
            _ = b.Property(i => i.InvoiceNumber).HasColumnName("invoice_number").IsRequired().HasMaxLength(255);
            _ = b.Property(i => i.Date).HasColumnName("date").IsRequired();
            _ = b.Property(i => i.DueDate).HasColumnName("due_date");
            _ = b.Property(i => i.Currency).HasColumnName("currency").IsRequired().HasMaxLength(255);
            _ = b.Property(i => i.ExchangeRate).HasColumnName("exchange_rate");
            _ = b.Property(i => i.Subtotal).HasColumnName("subtotal");
            _ = b.Property(i => i.DiscountAmount).HasColumnName("discount_amount");
            _ = b.Property(i => i.TotalNet).HasColumnName("total_net");
            _ = b.Property(i => i.TotalTax).HasColumnName("total_tax");
            _ = b.Property(i => i.TotalGross).HasColumnName("total_gross");
            _ = b.Property(i => i.RoundingAmount).HasColumnName("rounding_amount");
            _ = b.Property(i => i.TotalPaid).HasColumnName("total_paid");
            _ = b.Property(i => i.ReceivableAmount).HasColumnName("receivable_amount");
            _ = b.Property(i => i.PaymentStatus).HasColumnName("payment_status").IsRequired().HasMaxLength(255);
            _ = b.Property(i => i.Jurisdiction).HasColumnName("jurisdiction").IsRequired().HasMaxLength(255);
            _ = b.Property(i => i.Status).HasColumnName("status").IsRequired().HasMaxLength(255);
            _ = b.Property(i => i.SplitPayment).HasColumnName("split_payment");
            _ = b.Property(i => i.ReverseCharge).HasColumnName("reverse_charge");
            _ = b.Property(i => i.WithholdingTaxEnabled).HasColumnName("withholding_tax_enabled");
            _ = b.Property(i => i.WithholdingTaxPercent).HasColumnName("withholding_tax_percent");
            _ = b.Property(i => i.WithholdingTaxAmount).HasColumnName("withholding_tax_amount");
            _ = b.Property(i => i.StampDutyApplied).HasColumnName("stamp_duty_applied");
            _ = b.Property(i => i.StampDutyAmount).HasColumnName("stamp_duty_amount");
            _ = b.Property(i => i.FiscalYear).HasColumnName("fiscal_year");
            _ = b.Property(i => i.SequentialNumber).HasColumnName("sequential_number");
            _ = b.Property(i => i.PaymentMethod).HasColumnName("payment_method").HasMaxLength(255);
            _ = b.Property(i => i.BankIban).HasColumnName("bank_iban").HasMaxLength(255);
            _ = b.Property(i => i.Notes).HasColumnName("notes");
            _ = b.Property(i => i.PostedAt).HasColumnName("posted_at");
            _ = b.Property(i => i.CreatedAt).HasColumnName("created_at");
            _ = b.Property(i => i.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(i => i.DeletedAt).HasColumnName("deleted_at");

            _ = b.HasOne(i => i.Contact).WithMany().HasForeignKey(i => i.ContactId).OnDelete(DeleteBehavior.Restrict);
            _ = b.HasMany(i => i.Items).WithOne(it => it.Invoice).HasForeignKey(it => it.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            _ = b.HasOne(i => i.QrBillData).WithOne(q => q.Invoice).HasForeignKey<InvoiceQrBillData>(q => q.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            _ = b.HasOne(i => i.SdiMetadata).WithOne(s => s.Invoice).HasForeignKey<InvoiceSdiMetadata>(s => s.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            _ = b.HasQueryFilter(i => i.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<InvoiceItem>(b =>
        {
            _ = b.ToTable("invoice_items");
            _ = b.HasKey(it => it.Id);
            _ = b.Property(it => it.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(it => it.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(it => it.InvoiceId).HasColumnName("invoice_id").IsRequired().HasMaxLength(36);
            _ = b.Property(it => it.ProductId).HasColumnName("product_id").HasMaxLength(36);
            _ = b.Property(it => it.Description).HasColumnName("description").IsRequired().HasMaxLength(255);
            _ = b.Property(it => it.Quantity).HasColumnName("quantity");
            _ = b.Property(it => it.UnitPrice).HasColumnName("unit_price");
            _ = b.Property(it => it.TaxRate).HasColumnName("tax_rate");
            _ = b.Property(it => it.DiscountPercentage).HasColumnName("discount_percentage");
            _ = b.Property(it => it.LineTotal).HasColumnName("line_total");
            _ = b.Property(it => it.Uom).HasColumnName("uom").IsRequired().HasMaxLength(255);
            _ = b.Property(it => it.VatNature).HasColumnName("vat_nature").HasMaxLength(4);
            _ = b.Property(it => it.Notes).HasColumnName("notes");
            _ = b.Property(it => it.CreatedAt).HasColumnName("created_at");
            _ = b.Property(it => it.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(it => it.DeletedAt).HasColumnName("deleted_at");
            _ = b.HasOne(it => it.Product).WithMany().HasForeignKey(it => it.ProductId).OnDelete(DeleteBehavior.SetNull);
            _ = b.HasQueryFilter(it => it.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<InvoiceQrBillData>(b =>
        {
            _ = b.ToTable("invoice_qr_bill_data");
            _ = b.HasKey(q => q.Id);
            _ = b.Property(q => q.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(q => q.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            _ = b.Property(q => q.InvoiceId).HasColumnName("invoice_id").IsRequired().HasMaxLength(36);
            _ = b.Property(q => q.QrIban).HasColumnName("qr_iban").IsRequired().HasMaxLength(255);
            _ = b.Property(q => q.ReferenceType).HasColumnName("reference_type").IsRequired().HasMaxLength(4);
            _ = b.Property(q => q.PaymentReference).HasColumnName("payment_reference").IsRequired().HasMaxLength(27);
            _ = b.Property(q => q.CreditorInfo).HasColumnName("creditor_info").IsRequired();
            _ = b.Property(q => q.DebtorInfo).HasColumnName("debtor_info").IsRequired();
            _ = b.Property(q => q.QrPayload).HasColumnName("qr_payload");
            _ = b.Property(q => q.QrBillSvg).HasColumnName("qr_bill_svg");
            _ = b.Property(q => q.ValidationErrors).HasColumnName("validation_errors");
            _ = b.Property(q => q.CreatedAt).HasColumnName("created_at");
            _ = b.Property(q => q.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasQueryFilter(q => q.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<InvoiceSdiMetadata>(b =>
        {
            _ = b.ToTable("invoice_sdi_metadata");
            _ = b.HasKey(s => s.Id);
            _ = b.Property(s => s.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(s => s.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            _ = b.Property(s => s.InvoiceId).HasColumnName("invoice_id").IsRequired().HasMaxLength(36);
            _ = b.Property(s => s.TransmissionFormat).HasColumnName("transmission_format").IsRequired().HasMaxLength(255);
            _ = b.Property(s => s.SdiIdentifier).HasColumnName("sdi_identifier").HasMaxLength(255);
            _ = b.Property(s => s.SdiStatus).HasColumnName("sdi_status").IsRequired().HasMaxLength(255);
            _ = b.Property(s => s.FileName).HasColumnName("file_name").HasMaxLength(255);
            _ = b.Property(s => s.XmlContent).HasColumnName("xml_content");
            _ = b.Property(s => s.ValidationErrors).HasColumnName("validation_errors");
            _ = b.Property(s => s.SentAt).HasColumnName("sent_at");
            _ = b.Property(s => s.DeliveredAt).HasColumnName("delivered_at");
            _ = b.Property(s => s.CreatedAt).HasColumnName("created_at");
            _ = b.Property(s => s.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasQueryFilter(s => s.TenantId == currentTenantService.TenantId);
        });
    }

    private void ConfigureAccountingEntities(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<VatCode>(b =>
        {
            _ = b.ToTable("vat_codes");
            _ = b.HasKey(v => v.Id);
            _ = b.Property(v => v.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(v => v.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(v => v.Code).HasColumnName("code").IsRequired().HasMaxLength(255);
            _ = b.Property(v => v.Description).HasColumnName("description").HasMaxLength(255);
            _ = b.Property(v => v.CountryIso).HasColumnName("country_iso").IsRequired().HasMaxLength(255);
            _ = b.Property(v => v.Rate).HasColumnName("rate");
            _ = b.Property(v => v.TaxRatePercent).HasColumnName("tax_rate_percent");
            _ = b.Property(v => v.Nature).HasColumnName("nature").HasMaxLength(255);
            _ = b.Property(v => v.IsSplitPayment).HasColumnName("is_split_payment");
            _ = b.Property(v => v.IsReverseCharge).HasColumnName("is_reverse_charge");
            _ = b.Property(v => v.IsActive).HasColumnName("is_active");
            _ = b.Property(v => v.CreatedAt).HasColumnName("created_at");
            _ = b.Property(v => v.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(v => v.DeletedAt).HasColumnName("deleted_at");
            _ = b.HasQueryFilter(v => v.TenantId == null || v.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<ChartOfAccount>(b =>
        {
            _ = b.ToTable("chart_of_accounts");
            _ = b.HasKey(a => a.Id);
            _ = b.Property(a => a.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(a => a.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(a => a.Code).HasColumnName("code").HasMaxLength(255);
            _ = b.Property(a => a.AccountCode).HasColumnName("account_code").HasMaxLength(255);
            _ = b.Property(a => a.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            _ = b.Property(a => a.Type).HasColumnName("type").IsRequired().HasMaxLength(255);
            _ = b.Property(a => a.Standard).HasColumnName("standard").IsRequired().HasMaxLength(255);
            _ = b.Property(a => a.AccountClass).HasColumnName("account_class").IsRequired().HasMaxLength(255);
            _ = b.Property(a => a.AccountType).HasColumnName("account_type").IsRequired().HasMaxLength(255);
            _ = b.Property(a => a.NormalBalance).HasColumnName("normal_balance").IsRequired().HasMaxLength(255);
            _ = b.Property(a => a.Currency).HasColumnName("currency").IsRequired().HasMaxLength(255);
            _ = b.Property(a => a.AllowsManualPosting).HasColumnName("allows_manual_posting");
            _ = b.Property(a => a.IsActive).HasColumnName("is_active");
            _ = b.Property(a => a.CreatedAt).HasColumnName("created_at");
            _ = b.Property(a => a.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(a => a.DeletedAt).HasColumnName("deleted_at");
            _ = b.HasQueryFilter(a => a.TenantId == null || a.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<GeneralLedgerEntry>(b =>
        {
            _ = b.ToTable("general_ledger_entries");
            _ = b.HasKey(g => g.Id);
            _ = b.Property(g => g.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(g => g.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            _ = b.Property(g => g.EntryNumber).HasColumnName("entry_number").IsRequired().HasMaxLength(255);
            _ = b.Property(g => g.PostingDate).HasColumnName("posting_date").IsRequired();
            _ = b.Property(g => g.DocumentDate).HasColumnName("document_date").IsRequired();
            _ = b.Property(g => g.Description).HasColumnName("description").IsRequired();
            _ = b.Property(g => g.SourceDocumentType).HasColumnName("source_document_type").HasMaxLength(255);
            _ = b.Property(g => g.ReferenceId).HasColumnName("reference_id").HasMaxLength(36);
            _ = b.Property(g => g.IsLocked).HasColumnName("is_locked");
            _ = b.Property(g => g.CreatedAt).HasColumnName("created_at");
            _ = b.Property(g => g.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(g => g.DeletedAt).HasColumnName("deleted_at");

            _ = b.HasMany(g => g.Lines).WithOne(l => l.GeneralLedgerEntry).HasForeignKey(l => l.GeneralLedgerEntryId).OnDelete(DeleteBehavior.Cascade);
            _ = b.HasQueryFilter(g => g.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<GeneralLedgerLine>(b =>
        {
            _ = b.ToTable("general_ledger_lines");
            _ = b.HasKey(l => l.Id);
            _ = b.Property(l => l.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(l => l.GeneralLedgerEntryId).HasColumnName("general_ledger_entry_id").IsRequired().HasMaxLength(36);
            _ = b.Property(l => l.AccountId).HasColumnName("account_id").IsRequired().HasMaxLength(36);
            _ = b.Property(l => l.LineNumber).HasColumnName("line_number");
            _ = b.Property(l => l.Debit).HasColumnName("debit");
            _ = b.Property(l => l.Credit).HasColumnName("credit");
            _ = b.Property(l => l.Description).HasColumnName("description");
            _ = b.Property(l => l.CreatedAt).HasColumnName("created_at");
            _ = b.Property(l => l.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasOne(l => l.Account).WithMany().HasForeignKey(l => l.AccountId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureInventoryAndHrEntities(ModelBuilder modelBuilder)
    {
        _ = modelBuilder.Entity<HrEmployee>(b =>
        {
            _ = b.ToTable("hr_employees");
            _ = b.HasKey(e => e.Id);
            _ = b.Property(e => e.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(e => e.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            _ = b.Property(e => e.EmployeeNumber).HasColumnName("employee_number").IsRequired().HasMaxLength(255);
            _ = b.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(255);
            _ = b.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(255);
            _ = b.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
            _ = b.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(255);
            _ = b.Property(e => e.SocialSecurityNumber).HasColumnName("social_security_number").HasMaxLength(255);
            _ = b.Property(e => e.DepartmentName).HasColumnName("department_name").HasMaxLength(255);
            _ = b.Property(e => e.JobTitle).HasColumnName("job_title").HasMaxLength(255);
            _ = b.Property(e => e.EmploymentStatus).HasColumnName("employment_status").IsRequired().HasMaxLength(255);
            _ = b.Property(e => e.EmploymentType).HasColumnName("employment_type").IsRequired().HasMaxLength(255);
            _ = b.Property(e => e.HireDate).HasColumnName("hire_date").IsRequired();
            _ = b.Property(e => e.TerminationDate).HasColumnName("termination_date");
            _ = b.Property(e => e.BaseAnnualSalary).HasColumnName("base_annual_salary");
            _ = b.Property(e => e.HourlyCostRate).HasColumnName("hourly_cost_rate");
            _ = b.Property(e => e.Currency).HasColumnName("currency").IsRequired().HasMaxLength(255);
            _ = b.Property(e => e.FteRatio).HasColumnName("fte_ratio");
            _ = b.Property(e => e.WorkScheduleCode).HasColumnName("work_schedule_code").IsRequired().HasMaxLength(255);
            _ = b.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            _ = b.Property(e => e.SalaryInstallmentsPerYear).HasColumnName("salary_installments_per_year");
            _ = b.Property(e => e.IsFixedTermContract).HasColumnName("is_fixed_term_contract");
            _ = b.Property(e => e.WorkCanton).HasColumnName("work_canton").HasMaxLength(2);
            _ = b.Property(e => e.IsWithholdingTaxSubject).HasColumnName("is_withholding_tax_subject");
            _ = b.Property(e => e.WithholdingTaxCode).HasColumnName("withholding_tax_code").HasMaxLength(8);
            _ = b.Property(e => e.WithholdingTaxRatePercent).HasColumnName("withholding_tax_rate_percent");
            _ = b.Property(e => e.NonOccupationalAccidentRatePercent).HasColumnName("nbu_rate_percent");
            _ = b.Property(e => e.OccupationalAccidentRatePercent).HasColumnName("bu_rate_percent");
            _ = b.Property(e => e.DailySicknessRatePercent).HasColumnName("ktg_rate_percent");
            _ = b.Property(e => e.FamilyAllowanceFundRatePercent).HasColumnName("fak_rate_percent");
            _ = b.Property(e => e.PensionFundEmployeeRatePercent).HasColumnName("pension_fund_employee_rate_percent");
            _ = b.Property(e => e.RegionalSurchargeRatePercent).HasColumnName("regional_surcharge_rate_percent");
            _ = b.Property(e => e.MunicipalSurchargeRatePercent).HasColumnName("municipal_surcharge_rate_percent");
            _ = b.Property(e => e.Gender).HasColumnName("gender");
            _ = b.Property(e => e.Nationality).HasColumnName("nationality");
            _ = b.Property(e => e.ResidencePermit).HasColumnName("residence_permit");
            _ = b.Property(e => e.ResidenceCountry).HasColumnName("residence_country");
            _ = b.Property(e => e.ResidenceCanton).HasColumnName("residence_canton");
            _ = b.Property(e => e.Street).HasColumnName("street");
            _ = b.Property(e => e.HouseNumber).HasColumnName("house_number");
            _ = b.Property(e => e.PostalCode).HasColumnName("postal_code");
            _ = b.Property(e => e.City).HasColumnName("city");
            _ = b.Property(e => e.CivilStatus).HasColumnName("civil_status");
            _ = b.Property(e => e.ChurchTax).HasColumnName("church_tax");
            _ = b.Property(e => e.PayType).HasColumnName("pay_type");
            _ = b.Property(e => e.HourlyWage).HasColumnName("hourly_wage");
            _ = b.Property(e => e.WeeklyHours).HasColumnName("weekly_hours");
            _ = b.Property(e => e.HolidayWeeks).HasColumnName("holiday_weeks");
            _ = b.Property(e => e.Iban).HasColumnName("iban");
            _ = b.Property(e => e.TimeRecordingMode).HasColumnName("time_recording_mode");
            _ = b.Property(e => e.LegalMaxWeeklyHours).HasColumnName("legal_max_weekly_hours");
            _ = b.Property(e => e.WorkDaysPerWeek).HasColumnName("work_days_per_week");
            _ = b.Property(e => e.TimeRecordingAgreementDate).HasColumnName("time_recording_agreement_date");
            _ = b.Property(e => e.UserId).HasColumnName("user_id");
            _ = b.Property(e => e.CreatedAt).HasColumnName("created_at");
            _ = b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            _ = b.HasQueryFilter(e => e.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<Warehouse>(b =>
        {
            _ = b.ToTable("warehouses");
            _ = b.HasKey(w => w.Id);
            _ = b.Property(w => w.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(w => w.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(w => w.Code).HasColumnName("code").IsRequired().HasMaxLength(255);
            _ = b.Property(w => w.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            _ = b.Property(w => w.Address).HasColumnName("address").HasMaxLength(255);
            _ = b.Property(w => w.IsPrimary).HasColumnName("is_primary");
            _ = b.Property(w => w.WarehouseType).HasColumnName("warehouse_type").IsRequired().HasMaxLength(255);
            _ = b.Property(w => w.IsActive).HasColumnName("is_active");
            _ = b.Property(w => w.CreatedAt).HasColumnName("created_at");
            _ = b.Property(w => w.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(w => w.DeletedAt).HasColumnName("deleted_at");
            _ = b.HasQueryFilter(w => w.TenantId == null || w.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<StockItem>(b =>
        {
            _ = b.ToTable("stock_items");
            _ = b.HasKey(s => s.Id);
            _ = b.Property(s => s.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(s => s.TenantId).HasColumnName("tenant_id").IsRequired().HasMaxLength(36);
            _ = b.Property(s => s.Sku).HasColumnName("sku").IsRequired().HasMaxLength(255);
            _ = b.Property(s => s.BarcodeEan13).HasColumnName("barcode_ean13").HasMaxLength(255);
            _ = b.Property(s => s.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
            _ = b.Property(s => s.Description).HasColumnName("description");
            _ = b.Property(s => s.Uom).HasColumnName("uom").IsRequired().HasMaxLength(255);
            _ = b.Property(s => s.StandardCost).HasColumnName("standard_cost");
            _ = b.Property(s => s.CurrentMovingAverageCost).HasColumnName("current_moving_average_cost");
            _ = b.Property(s => s.WeightKg).HasColumnName("weight_kg");
            _ = b.Property(s => s.IsActive).HasColumnName("is_active");
            _ = b.Property(s => s.CreatedAt).HasColumnName("created_at");
            _ = b.Property(s => s.UpdatedAt).HasColumnName("updated_at");
            _ = b.Property(s => s.DeletedAt).HasColumnName("deleted_at");
            _ = b.HasQueryFilter(s => s.TenantId == currentTenantService.TenantId);
        });

        _ = modelBuilder.Entity<PaymentMethod>(b =>
        {
            _ = b.ToTable("payment_methods");
            _ = b.HasKey(m => m.Id);
            _ = b.Property(m => m.Id).HasColumnName("id").HasMaxLength(36);
            _ = b.Property(m => m.TenantId).HasColumnName("tenant_id").HasMaxLength(36);
            _ = b.Property(m => m.Code).HasColumnName("code").IsRequired().HasMaxLength(255);
            _ = b.Property(m => m.IsElectronic).HasColumnName("is_electronic");
            _ = b.Property(m => m.RequiresIban).HasColumnName("requires_iban");
            _ = b.Property(m => m.IsActive).HasColumnName("is_active");
            _ = b.Property(m => m.CreatedAt).HasColumnName("created_at");
            _ = b.Property(m => m.UpdatedAt).HasColumnName("updated_at");
            _ = b.HasQueryFilter(m => m.TenantId == null || m.TenantId == currentTenantService.TenantId);
        });
    }
}
