using System.Globalization;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

public sealed class ItalianElectronicInvoice : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public DateTime DocumentDate { get; private set; }

    /// <summary>
    /// FPR12 = B2B / Privati; FPA12 = Public Administration
    /// </summary>
    public string FormatoTrasmissione { get; } = "FPR12";

    /// <summary>
    /// Unique SDI submission counter
    /// </summary>
    public string ProgressivoInvio { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Header Data
    /// </summary>
    public string CedenteDenominazione { get; private set; } = string.Empty;
    public string CedentePartitaIva { get; private set; } = string.Empty;
    public string CedenteCodiceFiscale { get; private set; } = string.Empty;
    public Address CedenteIndirizzo { get; private set; } = null!;

    public string CessionarioDenominazione { get; private set; } = string.Empty;
    public string? CessionarioPartitaIva { get; private set; }
    public string? CessionarioCodiceFiscale { get; private set; }
    public string CessionarioCodiceDestinatario { get; private set; } = "0000000";
    public string? CessionarioPec { get; private set; }
    public Address CessionarioIndirizzo { get; private set; } = null!;

    /// <summary>
    /// Financial Totals
    /// </summary>
    public Currency Currency { get; } = Currency.EUR;
    public decimal TotalNet { get; private set; }
    public decimal TotalVat { get; private set; }
    public decimal TotalGross { get; private set; }

    /// <summary>
    /// MP05 = Bonifico
    /// </summary>
    public string PaymentMethodCode { get; } = "MP05";
    public DateTime PaymentDueDate { get; private set; }

    /// <summary>
    /// SDI Status
    /// </summary>
    public SdiTransmissionStatus SdiStatus { get; private set; } = SdiTransmissionStatus.Draft;
    public string? SdiIdentifier { get; private set; }
    public string? SdiErrorMessage { get; private set; }
    public DateTime? SdiSentAtUtc { get; private set; }
    public DateTime? SdiDeliveredAtUtc { get; private set; }

    private readonly List<InvoiceLine> _lines = [];
    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private ItalianElectronicInvoice()
    {
    }

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

    public void MarkSentToSdi(string sdiIdentifier)
    {
        SdiStatus = SdiTransmissionStatus.Sent;
        SdiIdentifier = sdiIdentifier;
        SdiSentAtUtc = DateTime.UtcNow;
    }

    public void MarkDelivered()
    {
        SdiStatus = SdiTransmissionStatus.Delivered;
        SdiDeliveredAtUtc = DateTime.UtcNow;
    }

    public void MarkRejected(string errorMessage)
    {
        SdiStatus = SdiTransmissionStatus.Rejected;
        SdiErrorMessage = errorMessage;
    }
}