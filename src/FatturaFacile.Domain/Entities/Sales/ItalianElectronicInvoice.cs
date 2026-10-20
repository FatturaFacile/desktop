using System.Globalization;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Represents italian electronic invoice.
/// </summary>
public sealed class ItalianElectronicInvoice : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the document number.
    /// </summary>
    public string DocumentNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the document date.
    /// </summary>
    public DateTime DocumentDate { get; private set; }

/// <summary>
    /// Gets the fPR12 = B2B / Privati; FPA12 = Public Administration.
    /// </summary>
    public string FormatoTrasmissione { get; } = "FPR12";

/// <summary>
    /// Gets the unique SDI submission counter.
    /// </summary>
    public string ProgressivoInvio { get; private set; } = string.Empty;

/// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

/// <summary>
    /// Gets the header Data.
    /// </summary>
    public string CedenteDenominazione { get; private set; } = string.Empty;

/// <summary>
    /// Gets the cedente partita iva.
    /// </summary>
    public string CedentePartitaIva { get; private set; } = string.Empty;

/// <summary>
    /// Gets the cedente codice fiscale.
    /// </summary>
    public string CedenteCodiceFiscale { get; private set; } = string.Empty;

/// <summary>
    /// Gets the cedente indirizzo.
    /// </summary>
    public Address CedenteIndirizzo { get; private set; } = null!;

/// <summary>
    /// Gets the cessionario denominazione.
    /// </summary>
    public string CessionarioDenominazione { get; private set; } = string.Empty;

/// <summary>
    /// Gets the cessionario partita iva.
    /// </summary>
    public string? CessionarioPartitaIva { get; private set; }

/// <summary>
    /// Gets the cessionario codice fiscale.
    /// </summary>
    public string? CessionarioCodiceFiscale { get; private set; }

/// <summary>
    /// Gets the cessionario codice destinatario.
    /// </summary>
    public string CessionarioCodiceDestinatario { get; private set; } = "0000000";

/// <summary>
    /// Gets the cessionario pec.
    /// </summary>
    public string? CessionarioPec { get; private set; }

/// <summary>
    /// Gets the cessionario indirizzo.
    /// </summary>
    public Address CessionarioIndirizzo { get; private set; } = null!;

/// <summary>
    /// Gets the financial Totals.
    /// </summary>
    public Currency Currency { get; } = Currency.EUR;

/// <summary>
    /// Gets the total net.
    /// </summary>
    public decimal TotalNet { get; private set; }

/// <summary>
    /// Gets the total vat.
    /// </summary>
    public decimal TotalVat { get; private set; }

/// <summary>
    /// Gets the total gross.
    /// </summary>
    public decimal TotalGross { get; private set; }

/// <summary>
    /// Gets the mP05 = Bonifico.
    /// </summary>
    public string PaymentMethodCode { get; } = "MP05";

/// <summary>
    /// Gets the payment due date.
    /// </summary>
    public DateTime PaymentDueDate { get; private set; }

/// <summary>
    /// Gets the sDI Status.
    /// </summary>
    public SdiTransmissionStatus SdiStatus { get; private set; } = SdiTransmissionStatus.Draft;

/// <summary>
    /// Gets the sdi identifier.
    /// </summary>
    public string? SdiIdentifier { get; private set; }

/// <summary>
    /// Gets the sdi error message.
    /// </summary>
    public string? SdiErrorMessage { get; private set; }

/// <summary>
    /// Gets the sdi sent at utc.
    /// </summary>
    public DateTime? SdiSentAtUtc { get; private set; }

/// <summary>
    /// Gets the sdi delivered at utc.
    /// </summary>
    public DateTime? SdiDeliveredAtUtc { get; private set; }

private readonly List<InvoiceLine> _lines = [];

/// <summary>
    /// Gets the lines.
    /// </summary>
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

/// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

/// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

/// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

private ItalianElectronicInvoice()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="documentNumber">The document number.</param>
    /// <param name="documentDate">The document date.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="cedenteName">The cedente name.</param>
    /// <param name="cedenteVat">The cedente vat.</param>
    /// <param name="cedenteCf">The cedente cf.</param>
    /// <param name="cedenteAddress">The cedente address.</param>
    /// <param name="cessionarioName">The cessionario name.</param>
    /// <param name="cessionarioVat">The cessionario vat.</param>
    /// <param name="cessionarioCf">The cessionario cf.</param>
    /// <param name="sdiCode">The sdi code.</param>
    /// <param name="pec">The pec.</param>
    /// <param name="cessionarioAddress">The cessionario address.</param>
    /// <param name="dueDate">The due date.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static ItalianElectronicInvoice Create(
        Guid tenantId,
        string documentNumber,
        DateTime documentDate,
        Guid clientId,
        string cedenteName,
        string cedenteVat,
        string cedenteCf,
        Address cedenteAddress,
        string cessionarioName,
        string? cessionarioVat,
        string? cessionarioCf,
        string sdiCode,
        string? pec,
        Address cessionarioAddress,
        DateTime dueDate)
    {
        return new ItalianElectronicInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            DocumentNumber = documentNumber,
            DocumentDate = documentDate,
            ProgressivoInvio = string.Create(CultureInfo.InvariantCulture, $"{DateTime.UtcNow:yyyyMMddHHmmssfff}"),
            ClientId = clientId,
            CedenteDenominazione = cedenteName,
            CedentePartitaIva = cedenteVat,
            CedenteCodiceFiscale = cedenteCf,
            CedenteIndirizzo = cedenteAddress,
            CessionarioDenominazione = cessionarioName,
            CessionarioPartitaIva = cessionarioVat,
            CessionarioCodiceFiscale = cessionarioCf,
            CessionarioCodiceDestinatario = string.IsNullOrWhiteSpace(sdiCode) ? "0000000" : sdiCode.Trim().ToUpperInvariant(),
            CessionarioPec = pec,
            CessionarioIndirizzo = cessionarioAddress,
            PaymentDueDate = dueDate,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

/// <summary>
    /// Add Line.
    /// </summary>
    /// <param name="description">The description.</param>
    /// <param name="quantity">The quantity.</param>
    /// <param name="unitPrice">The unit price.</param>
    /// <param name="vatRate">The vat rate.</param>
    /// <param name="natura">The natura.</param>
    public void AddLine(string description, decimal quantity, decimal unitPrice, decimal vatRate, string? natura = null)
    {
        var line = new InvoiceLine(_lines.Count + 1, description, quantity, unitPrice, vatRate, natura);
        _lines.Add(line);
        RecalculateTotals();
    }

private void RecalculateTotals()
    {
        TotalNet = _lines.Sum(l => l.LineTotal);
        TotalVat = _lines.Sum(l => decimal.Round(l.LineTotal * (l.VatRate / 100m), 2, MidpointRounding.AwayFromZero));
        TotalGross = TotalNet + TotalVat;
    }

/// <summary>
    /// Mark Sent To Sdi.
    /// </summary>
    /// <param name="sdiIdentifier">The sdi identifier.</param>
    public void MarkSentToSdi(string sdiIdentifier)
    {
        SdiStatus = SdiTransmissionStatus.Sent;
        SdiIdentifier = sdiIdentifier;
        SdiSentAtUtc = DateTime.UtcNow;
    }

/// <summary>
    /// Mark Delivered.
    /// </summary>
    public void MarkDelivered()
    {
        SdiStatus = SdiTransmissionStatus.Delivered;
        SdiDeliveredAtUtc = DateTime.UtcNow;
    }

/// <summary>
    /// Mark Rejected.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    public void MarkRejected(string errorMessage)
    {
        SdiStatus = SdiTransmissionStatus.Rejected;
        SdiErrorMessage = errorMessage;
    }
}
