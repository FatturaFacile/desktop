// <copyright file="BuiltInTemplates.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Accounting;

namespace FatturaFacile.Application.Accounting;

/// <summary>
/// The templates available before the first sync, so the Prima Nota works offline.
/// </summary>
public static class BuiltInTemplates
{
    private static readonly Guid SalesInvoiceId = new(0xA0001, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    private static readonly Guid PurchaseInvoiceId = new(0xA0002, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    private static readonly Guid CustomerPaymentId = new(0xA0003, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    /// <summary>
    /// Gets the built-in templates with their names in the given language.
    /// </summary>
    /// <param name="language">The language: it, de, fr or en.</param>
    /// <returns>The templates.</returns>
    public static IReadOnlyList<TemplateDefinition> All(string language) =>
    [
        new TemplateDefinition(
            SalesInvoiceId,
            "SALES_INVOICE",
            Name(language, it: "Fattura di vendita", de: "Ausgangsrechnung", fr: "Facture de vente", en: "Sales invoice"),
            LedgerBook.Statutory,
            [
                new TemplateParameter("vat_rate", "rate"),
                new TemplateParameter("receivable_account", "account"),
                new TemplateParameter("revenue_account", "account"),
                new TemplateParameter("vat_account", "account"),
            ],
            [
                new TemplateLineDefinition("$receivable_account", "debit", "gross", Description: null),
                new TemplateLineDefinition("$revenue_account", "credit", "amount", Description: null),
                new TemplateLineDefinition("$vat_account", "credit", "tax", Description: null),
            ]),
        new TemplateDefinition(
            PurchaseInvoiceId,
            "PURCHASE_INVOICE",
            Name(language, it: "Fattura di acquisto", de: "Eingangsrechnung", fr: "Facture d'achat", en: "Purchase invoice"),
            LedgerBook.Statutory,
            [
                new TemplateParameter("vat_rate", "rate"),
                new TemplateParameter("expense_account", "account"),
                new TemplateParameter("vat_account", "account"),
                new TemplateParameter("payable_account", "account"),
            ],
            [
                new TemplateLineDefinition("$expense_account", "debit", "amount", Description: null),
                new TemplateLineDefinition("$vat_account", "debit", "tax", Description: null),
                new TemplateLineDefinition("$payable_account", "credit", "gross", Description: null),
            ]),
        new TemplateDefinition(
            CustomerPaymentId,
            "CUSTOMER_PAYMENT",
            Name(language, it: "Incasso cliente", de: "Kundenzahlung", fr: "Encaissement client", en: "Customer payment"),
            LedgerBook.Statutory,
            [
                new TemplateParameter("bank_account", "account"),
                new TemplateParameter("receivable_account", "account"),
            ],
            [
                new TemplateLineDefinition("$bank_account", "debit", "amount", Description: null),
                new TemplateLineDefinition("$receivable_account", "credit", "amount", Description: null),
            ]),
    ];

    private static string Name(string language, string it, string de, string fr, string en) => language switch
    {
        "de" => de,
        "fr" => fr,
        "en" => en,
        _ => it,
    };
}
