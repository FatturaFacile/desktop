// <copyright file="VatComplianceModelConfiguration.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FatturaFacile.Infrastructure.VatCompliance;

/// <summary>
/// EF Core mapping of the Italian VAT compliance tables (Mod 029): registers, print audit, declarations of intent, LIPE
/// communications, settlements, withholdings and Certificazioni Uniche. The tenant filters are added by the database context
/// itself, so they read the tenant service of each context instance.
/// </summary>
public static class VatComplianceModelConfiguration
{
    private const int GuidLength = 36;
    private const int ShortText = 40;
    private const int NameText = 200;
    private const int HashLength = 64;
    private const int LongText = 2000;

    /// <summary>
    /// Maps the VAT compliance tables.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    public static void ConfigureVatCompliance(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.Entity<VatRegisterEntry>(b =>
        {
            _ = b.ToTable("vat_register_entries");
            Key(b);
            _ = b.Property(x => x.DocumentNumber).HasColumnName("document_number").IsRequired().HasMaxLength(ShortText);
            _ = b.Property(x => x.PartnerName).HasColumnName("partner_name").HasMaxLength(NameText);
            _ = b.Property(x => x.PartnerVatNumber).HasColumnName("partner_vat_number").HasMaxLength(ShortText);
            _ = b.Property(x => x.Description).HasColumnName("description").HasMaxLength(LongText);
            _ = b.Property(x => x.VatNature).HasColumnName("vat_nature").HasMaxLength(ShortText);
            _ = b.Property(x => x.SourceType).HasColumnName("source_type").IsRequired().HasMaxLength(ShortText);
            _ = b.Property(x => x.SourceId).HasColumnName("source_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.PrintRunId).HasColumnName("print_run_id").HasMaxLength(GuidLength);
            _ = b.HasIndex(x => new { x.TenantId, x.Kind, x.Section, x.Year, x.ProtocolNumber });
            _ = b.HasIndex(x => new { x.TenantId, x.Kind, x.SourceType, x.SourceId, x.VatRate, x.VatNature }).IsUnique();
        });

        _ = modelBuilder.Entity<VatRegisterPrintRun>(b =>
        {
            _ = b.ToTable("vat_register_print_runs");
            Key(b);
            _ = b.Property(x => x.ContentSha256).HasColumnName("content_sha256").IsRequired().HasMaxLength(HashLength);
            _ = b.Property(x => x.VirtualStampReference).HasColumnName("virtual_stamp_reference").HasMaxLength(NameText);
            _ = b.Property(x => x.PrintedBy).HasColumnName("printed_by").IsRequired().HasMaxLength(NameText);
            _ = b.HasIndex(x => new { x.TenantId, x.Kind, x.Section, x.Year });
        });

        _ = modelBuilder.Entity<IntentLetter>(b =>
        {
            _ = b.ToTable("intent_letters");
            Key(b);
            _ = b.Property(x => x.ContactId).HasColumnName("contact_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.ProtocolNumber).HasColumnName("protocol_number").IsRequired().HasMaxLength(ShortText);
            _ = b.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(LongText);
            _ = b.Property(x => x.ServerId).HasColumnName("server_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.SyncError).HasColumnName("sync_error").HasMaxLength(LongText);
            _ = b.HasIndex(x => new { x.TenantId, x.ContactId, x.Year });
            _ = b.HasIndex(x => new { x.TenantId, x.ProtocolNumber });
        });

        _ = modelBuilder.Entity<IntentLetterUsage>(b =>
        {
            _ = b.ToTable("intent_letter_usages");
            Key(b);
            _ = b.Property(x => x.IntentLetterId).HasColumnName("intent_letter_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.InvoiceId).HasColumnName("invoice_id").HasMaxLength(GuidLength);
            _ = b.HasIndex(x => new { x.TenantId, x.IntentLetterId });
            _ = b.HasIndex(x => new { x.TenantId, x.InvoiceId }).IsUnique();
        });

        _ = modelBuilder.Entity<VatSettlementSnapshot>(b =>
        {
            _ = b.ToTable("vat_settlement_snapshots");
            Key(b);
            _ = b.Property(x => x.ServerId).HasColumnName("server_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.PeriodType).HasColumnName("period_type").IsRequired().HasMaxLength(ShortText);
            _ = b.Property(x => x.Status).HasColumnName("status").IsRequired().HasMaxLength(ShortText);
            _ = b.Property(x => x.SettlementJournalId).HasColumnName("settlement_journal_id").HasMaxLength(GuidLength);
            _ = b.HasIndex(x => new { x.TenantId, x.ServerId }).IsUnique();
            _ = b.HasIndex(x => new { x.TenantId, x.Year });
        });

        _ = modelBuilder.Entity<LipeSubmission>(b =>
        {
            _ = b.ToTable("lipe_submissions");
            Key(b);
            _ = b.Property(x => x.VatReturnId).HasColumnName("vat_return_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.FileName).HasColumnName("file_name").IsRequired().HasMaxLength(NameText);
            _ = b.Property(x => x.XmlContent).HasColumnName("xml_content").IsRequired();
            _ = b.Property(x => x.XmlSha256).HasColumnName("xml_sha256").IsRequired().HasMaxLength(HashLength);
            _ = b.Property(x => x.ValidationErrors).HasColumnName("validation_errors");
            _ = b.Property(x => x.SignedFileName).HasColumnName("signed_file_name").HasMaxLength(NameText);
            _ = b.Property(x => x.ReceiptProtocol).HasColumnName("receipt_protocol").HasMaxLength(NameText);
            _ = b.Property(x => x.ReceiptNote).HasColumnName("receipt_note").HasMaxLength(LongText);
            _ = b.HasIndex(x => new { x.TenantId, x.VatReturnId });
        });

        _ = modelBuilder.Entity<WithholdingLine>(b =>
        {
            _ = b.ToTable("withholding_lines");
            Key(b);
            _ = b.Property(x => x.SettlementServerId).HasColumnName("settlement_server_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.SupplierId).HasColumnName("supplier_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.InvoiceNumber).HasColumnName("invoice_number").IsRequired().HasMaxLength(ShortText);
            _ = b.Property(x => x.Reason).HasColumnName("reason").IsRequired().HasMaxLength(ShortText);
            _ = b.Property(x => x.F24Code).HasColumnName("f24_code").IsRequired().HasMaxLength(ShortText);
            _ = b.HasIndex(x => new { x.TenantId, x.Year, x.SupplierId });
            _ = b.HasIndex(x => new { x.TenantId, x.SettlementServerId });
        });

        _ = modelBuilder.Entity<WithholdingCertificate>(b =>
        {
            _ = b.ToTable("withholding_certificates");
            Key(b);
            _ = b.Property(x => x.SupplierId).HasColumnName("supplier_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.RecipientName).HasColumnName("recipient_name").IsRequired().HasMaxLength(NameText);
            _ = b.Property(x => x.RecipientTaxCode).HasColumnName("recipient_tax_code").IsRequired().HasMaxLength(ShortText);
            _ = b.Property(x => x.RecipientVatNumber).HasColumnName("recipient_vat_number").HasMaxLength(ShortText);
            _ = b.Property(x => x.Reason).HasColumnName("reason").IsRequired().HasMaxLength(ShortText);
            _ = b.HasIndex(x => new { x.TenantId, x.Year, x.SupplierId, x.Reason }).IsUnique();
        });

        _ = modelBuilder.Entity<WithholdingF24Payment>(b =>
        {
            _ = b.ToTable("withholding_f24_payments");
            Key(b);
            _ = b.Property(x => x.TaxCode).HasColumnName("tax_code").IsRequired().HasMaxLength(ShortText);
            _ = b.Property(x => x.PaymentReference).HasColumnName("payment_reference").HasMaxLength(NameText);
            _ = b.Property(x => x.SettlementServerId).HasColumnName("settlement_server_id").HasMaxLength(GuidLength);
            _ = b.Property(x => x.SyncError).HasColumnName("sync_error").HasMaxLength(LongText);
            _ = b.HasIndex(x => new { x.TenantId, x.Year, x.Month, x.TaxCode });
        });
    }

    private static void Key<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> builder)
        where T : class, FatturaFacile.Domain.Common.ITenantScopedEntity
    {
        _ = builder.HasKey(x => x.Id);
        _ = builder.Property(x => x.Id).HasColumnName("id").HasMaxLength(GuidLength);
        _ = builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(GuidLength);
        _ = builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        _ = builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}
