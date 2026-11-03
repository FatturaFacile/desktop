// <copyright file="ItalianElectronicInvoice.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents an Italian electronic invoice (FatturaPA / Fattura Elettronica) domain aggregate for B2B, B2C,
/// and Public Administration transactions regulated by the Italian Revenue Agency (Agenzia delle Entrate) and routed via the SDI exchange system (Sistema di Interscambio).
/// </summary>
public sealed class ItalianElectronicInvoice : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private const string DefaultRecipientCode = "0000000";
    private const decimal PercentageDivisor = 100m;
    private const int CurrencyDecimals = 2;

    /// <summary>
    /// The internal collection of invoice line items.
    /// </summary>
    private readonly List<InvoiceLine> lines = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ItalianElectronicInvoice"/> class.
    /// </summary>
    private ItalianElectronicInvoice()
    {
    }

    /// <summary>
    /// Gets or sets the unique tenant identifier for multi-tenant data partitioning.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique invoice document reference number within the fiscal year or document sequence (element Numero in FatturaPA, e.g. "2026/001").
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the document issuance date (element Data in FatturaPA).
    /// </summary>
    public DateTime DocumentDate { get; private set; }

    /// <summary>
    /// Gets the transmission format identifier mandated by SDI: "FPR12" for private/B2B/B2C transactions, or "FPA12" for public administration entities. Defaults to "FPR12".
    /// </summary>
    public string FormatoTrasmissione { get; } = "FPR12";

    /// <summary>
    /// Gets the unique transmission progress counter for the sending file (element ProgressivoInvio in DatiTrasmissione), uniquely indexing each SDI dispatch.
    /// </summary>
    public string ProgressivoInvio { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the internal unique identifier of the customer (cessionario/committente) associated with this sales document.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the business or corporate trade name of the supplier/issuer (Cedente/Prestatore element Denominazione).
    /// </summary>
    public string CedenteDenominazione { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the Italian VAT identification number (Partita IVA) of the supplier/issuer, prefixed by country code (e.g., IT01234567890).
    /// </summary>
    public string CedentePartitaIva { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the Italian fiscal code (Codice Fiscale) of the supplier/issuer (16 alphanumeric characters for individuals, 11 digits for entities).
    /// </summary>
    public string CedenteCodiceFiscale { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the registered legal or fiscal headquarters address of the supplier/issuer (Cedente/Prestatore).
    /// </summary>
    public Address CedenteIndirizzo { get; private set; } = null!;

    /// <summary>
    /// Gets the business, corporate, or individual name of the buyer/customer (Cessionario/Committente element Denominazione).
    /// </summary>
    public string CessionarioDenominazione { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the Italian or EU VAT registration number (Partita IVA) of the buyer/customer, if registered for VAT.
    /// </summary>
    public string? CessionarioPartitaIva { get; private set; }

    /// <summary>
    /// Gets the Italian fiscal code (Codice Fiscale) of the buyer/customer, mandatory for private consumers (B2C) and Italian corporate buyers.
    /// </summary>
    public string? CessionarioCodiceFiscale { get; private set; }

    /// <summary>
    /// Gets the SDI recipient routing code (Codice Destinatario), consisting of 7 alphanumeric characters
    /// for B2B/B2C intermediary channels, 6 characters for Public Administrations (CUU),
    /// or "0000000" when dispatching via certified email (PEC).
    /// </summary>
    public string CessionarioCodiceDestinatario { get; private set; } = "0000000";

    /// <summary>
    /// Gets the certified electronic email address (Posta Elettronica Certificata - PEC) of the buyer, used by SDI for delivery when <see cref="CessionarioCodiceDestinatario"/> is "0000000".
    /// </summary>
    public string? CessionarioPec { get; private set; }

    /// <summary>
    /// Gets the billing or registered address of the buyer/customer (Cessionario/Committente).
    /// </summary>
    public Address CessionarioIndirizzo { get; private set; } = null!;

    /// <summary>
    /// Gets the ISO currency code for the invoice totals and line items; defaults to EUR.
    /// </summary>
    public Currency Currency { get; } = Currency.EUR;

    /// <summary>
    /// Gets the cumulative taxable net amount of all invoice lines, excluding VAT.
    /// </summary>
    public decimal TotalNet { get; private set; }

    /// <summary>
    /// Gets the aggregate VAT amount across all invoice lines.
    /// </summary>
    public decimal TotalVat { get; private set; }

    /// <summary>
    /// Gets the gross total document amount payable by the customer, inclusive of VAT (element ImportoTotaleDocumento).
    /// </summary>
    public decimal TotalGross { get; private set; }

    /// <summary>
    /// Gets the statutory payment method code defined by FatturaPA specifications (element ModalitaPagamento, e.g. "MP05" for wire transfer / bonifico bancario).
    /// </summary>
    public string PaymentMethodCode { get; } = "MP05";

    /// <summary>
    /// Gets the contractual maturity or due date for invoice settlement.
    /// </summary>
    public DateTime PaymentDueDate { get; private set; }

    /// <summary>
    /// Gets the current SDI transmission and lifecycle status (e.g. Draft, Sent, Delivered, Rejected).
    /// </summary>
    public SdiTransmissionStatus SdiStatus { get; private set; } = SdiTransmissionStatus.Draft;

    /// <summary>
    /// Gets the official transaction identifier assigned by the SDI interchange system (IdentificativoSdI) upon ingestion.
    /// </summary>
    public string? SdiIdentifier { get; private set; }

    /// <summary>
    /// Gets the detailed error message or discard reason returned by SDI in the notification of rejection (Notifica di Scarto - NS).
    /// </summary>
    public string? SdiErrorMessage { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp indicating when the XML invoice payload was dispatched to SDI.
    /// </summary>
    public DateTime? SdiSentAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC timestamp indicating when the SDI exchange confirmed delivery to the recipient via delivery receipt (Ricevuta di Consegna - RC).
    /// </summary>
    public DateTime? SdiDeliveredAtUtc { get; private set; }

    /// <summary>
    /// Gets the read-only collection of itemized billing lines belonging to this electronic invoice.
    /// </summary>
    public IReadOnlyCollection<InvoiceLine> Lines => lines.AsReadOnly();

    /// <summary>
    /// Gets or sets the UTC timestamp when the invoice record was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or system process that created the invoice record.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the invoice record was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or system process that last updated the invoice record.
    /// </summary>
    public string? UpdatedBy { get; set; }

   /// <summary>
   /// Creates a new <see cref="ItalianElectronicInvoice"/> instance initialized in draft state with the specified tenant, client, invoice header, supplier, and customer information.
   /// </summary>
   /// <param name="tenantId">The unique tenant identifier for isolation.</param>
   /// <param name="clientId">The customer identifier in the local CRM/ledger.</param>
   /// <param name="header">The invoice header containing document number, document date, and due date.</param>
   /// <param name="supplier">The supplier information for the invoice.</param>
   /// <param name="customer">The customer information for the invoice.</param>
   /// <returns>A newly configured <see cref="ItalianElectronicInvoice"/> aggregate ready for line addition and SDI transmission.</returns>
    public static ItalianElectronicInvoice Create(
     Guid tenantId,
     Guid clientId,
     InvoiceHeader header,
     SupplierInfo supplier,
     CustomerInfo customer)
    {
        ArgumentNullException.ThrowIfNull(header);
        ArgumentNullException.ThrowIfNull(supplier);
        ArgumentNullException.ThrowIfNull(customer);

        return new ItalianElectronicInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,

            DocumentNumber = header.DocumentNumber,
            DocumentDate = header.DocumentDate,
            PaymentDueDate = header.DueDate,

            ProgressivoInvio = string.Create(
                CultureInfo.InvariantCulture,
                $"{DateTime.UtcNow:yyyyMMddHHmmssfff}"),

            ClientId = clientId,

            CedenteDenominazione = supplier.Name,
            CedentePartitaIva = supplier.VatNumber,
            CedenteCodiceFiscale = supplier.FiscalCode,
            CedenteIndirizzo = supplier.Address,

            CessionarioDenominazione = customer.Name,
            CessionarioPartitaIva = customer.VatNumber,
            CessionarioCodiceFiscale = customer.FiscalCode,

            CessionarioCodiceDestinatario =
                string.IsNullOrWhiteSpace(customer.RecipientCode)
                    ? DefaultRecipientCode
                    : customer.RecipientCode.Trim().ToUpperInvariant(),

            CessionarioPec = customer.Pec,
            CessionarioIndirizzo = customer.Address,

            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Appends an itemized invoice line to the electronic invoice and updates the net, VAT, and gross document totals.
    /// </summary>
    /// <param name="description">The description of the product delivered or service performed.</param>
    /// <param name="quantity">The billed quantity.</param>
    /// <param name="unitPrice">The net unit price before tax.</param>
    /// <param name="vatRate">The VAT rate percentage (e.g. 22.0 or 0.0).</param>
    /// <param name="natura">The statutory VAT exemption code (e.g. "N1", "N2.2", "N3.1", "N4") if the VAT rate is zero; otherwise <see langword="null"/>.</param>
    public void AddLine(
        string  description,
        decimal quantity,
        decimal unitPrice,
        decimal vatRate,
        string? natura = null)
    {
        var line = new InvoiceLine(
                                   lineNumber: lines.Count + 1,
                                   description: description,
                                   quantity: quantity,
                                   unitPrice: unitPrice,
                                   vatRate: vatRate,
                                   natura: natura);

        lines.Add(item: line);
        RecalculateTotals();
    }

    /// <summary>
    /// Updates the invoice state to indicate that the document payload has been successfully dispatched to the SDI exchange.
    /// </summary>
    /// <param name="sdiIdentifier">The unique transmission receipt identifier (IdentificativoSdI) assigned by Agenzia delle Entrate.</param>
    public void MarkSentToSdi(string sdiIdentifier)
    {
        SdiStatus     = SdiTransmissionStatus.Sent;
        SdiIdentifier = sdiIdentifier;
        SdiSentAtUtc  = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the invoice state to record that the SDI interchange delivered the electronic invoice to the recipient's digital mailbox or SDI endpoint (Ricevuta di Consegna).
    /// </summary>
    public void MarkDelivered()
    {
        SdiStatus         = SdiTransmissionStatus.Delivered;
        SdiDeliveredAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the invoice state to record an SDI transmission failure or rejection notification (Notifica di Scarto) along with the explanatory diagnostics.
    /// </summary>
    /// <param name="errorMessage">The error code or descriptive reason provided by the SDI validation engine (e.g. invalid tax code or schema violation).</param>
    public void MarkRejected(string errorMessage)
    {
        SdiStatus       = SdiTransmissionStatus.Rejected;
        SdiErrorMessage = errorMessage;
    }

    /// <summary>
    /// Recalculates the invoice totals based on the current line items.
    /// </summary>
    private void RecalculateTotals()
    {
        TotalNet = lines.Sum(static l => l.LineTotal);

        TotalVat = lines.Sum(
            static l => decimal.Round(
                l.LineTotal * (l.VatRate / PercentageDivisor),
                CurrencyDecimals,
                MidpointRounding.AwayFromZero));

        TotalGross = TotalNet + TotalVat;
    }

}
