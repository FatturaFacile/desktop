// <copyright file="ErpModulesTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Assets;
using FatturaFacile.Application.Banking;
using FatturaFacile.Application.Purchasing;
using FatturaFacile.Application.Taxation;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of supplier invoices, bank statements, VAT returns and depreciation.
/// </summary>
public sealed class ErpModulesTests
{
    private const string Camt053 =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<Document xmlns=\"urn:iso:std:iso:20022:tech:xsd:camt.053.001.08\">" +
        "<BkToCstmrStmt>" +
        "<Stmt>" +
        "<Acct><Id><IBAN>CH5604835012345678009</IBAN></Id></Acct>" +
        "<Ntry>" +
        "<Amt Ccy=\"CHF\">1120.00</Amt>" +
        "<CdtDbtInd>CRDT</CdtDbtInd>" +
        "<BookgDt><Dt>2026-11-04</Dt></BookgDt>" +
        "<ValDt><Dt>2026-11-04</Dt></ValDt>" +
        "<AcctSvcrRef>REF-001</AcctSvcrRef>" +
        "<NtryDtls><TxDtls>" +
        "<Amt Ccy=\"CHF\">1120.00</Amt>" +
        "<RltdPties><Dbtr><Pty><Nm>Cliente SA</Nm></Pty></Dbtr><DbtrAcct><Id><IBAN>CH9300762011623852957</IBAN></Id></DbtrAcct></RltdPties>" +
        "<RmtInf><Strd><CdtrRefInf><Ref>210000000003139471430009017</Ref></CdtrRefInf></Strd></RmtInf>" +
        "</TxDtls></NtryDtls>" +
        "</Ntry>" +
        "<Ntry>" +
        "<Amt Ccy=\"CHF\">350.50</Amt>" +
        "<CdtDbtInd>DBIT</CdtDbtInd>" +
        "<BookgDt><Dt>2026-11-05</Dt></BookgDt>" +
        "<AcctSvcrRef>REF-002</AcctSvcrRef>" +
        "<AddtlNtryInf>Pagamento fornitore 4711</AddtlNtryInf>" +
        "</Ntry>" +
        "</Stmt>" +
        "</BkToCstmrStmt>" +
        "</Document>";

    /// <summary>
    /// The input VAT is reduced by the non-deductible share.
    /// </summary>
    [Fact]
    public void PurchaseInvoice_ComputesDeductibleVat()
    {
        PurchaseInvoice invoice = new();
        List<PurchaseInvoiceLine> lines =
        [
            new PurchaseInvoiceLine { Description = "Materiale", NetAmount = 1000m, VatRate = 8.1m },
            new PurchaseInvoiceLine { Description = "Auto (uso misto)", NetAmount = 200m, VatRate = 8.1m, DeductiblePercent = 50m },
        ];

        PurchaseInvoiceCalculator.Compute(invoice, lines);

        Assert.Equal(1200m, invoice.TotalNet);
        Assert.Equal(97.20m, invoice.TotalVat);
        Assert.Equal(89.10m, invoice.DeductibleVat);
        Assert.Equal(1297.20m, invoice.TotalGross);
    }

    /// <summary>
    /// camt.053 entries become signed movements with reference, counterparty and remittance text.
    /// </summary>
    [Fact]
    public void Camt053_ParsesCreditsAndDebits()
    {
        var (iban, transactions) = CamtStatementParser.Parse(Camt053);

        Assert.Equal("CH5604835012345678009", iban);
        Assert.Equal(2, transactions.Count);
        Assert.Equal(1120m, transactions[0].Amount);
        Assert.Equal("210000000003139471430009017", transactions[0].Reference);
        Assert.Equal("Cliente SA", transactions[0].Counterparty);
        Assert.Equal("CH9300762011623852957", transactions[0].CounterpartyIban);
        Assert.Equal("REF-001", transactions[0].ExternalId);
        Assert.Equal(-350.50m, transactions[1].Amount);
        Assert.Equal("Pagamento fornitore 4711", transactions[1].Description);
        Assert.Equal("210000000003139471430009017", PaymentReferenceNormalizer.Normalize("21 00000 00003 13947 14300 09017"));
        _ = Assert.Throws<FormatException>(() => CamtStatementParser.Parse("<Document />"));
    }

    /// <summary>
    /// Swiss MWST statement with the effective method.
    /// </summary>
    [Fact]
    public void VatReturn_Swiss_EffectiveMethod()
    {
        var from = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var result = VatReturnCalculator.Compute(
            "CH",
            from,
            from.AddMonths(6).AddDays(-1),
            [(new Invoice { TotalNet = 1000m }, [new InvoiceItem { LineTotal = 1000m, TaxRate = 8.1m }])],
            [new PurchaseInvoice { DeductibleVat = 50m }],
            previousCredit: 0m);

        Assert.Equal(1000m, result.Turnover);
        Assert.Equal(81m, result.OutputVat);
        Assert.Equal(50m, result.InputVat);
        Assert.Equal(31m, result.Payable);
        Assert.Equal(0m, result.Interest);
    }

    /// <summary>
    /// Italian quarterly liquidation: split payment excluded, previous credit, 1 % interest.
    /// </summary>
    [Fact]
    public void VatReturn_Italian_QuarterlyWithSplitPaymentAndInterest()
    {
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var result = VatReturnCalculator.Compute(
            "IT",
            from,
            new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Unspecified),
            [
                (new Invoice { TotalNet = 1000m }, [new InvoiceItem { LineTotal = 1000m, TaxRate = 22m }]),
                (new Invoice { TotalNet = 500m, SplitPayment = true }, [new InvoiceItem { LineTotal = 500m, TaxRate = 22m }]),
            ],
            [new PurchaseInvoice { DeductibleVat = 100m }],
            previousCredit: 20m);

        Assert.Equal(220m, result.OutputVat);
        Assert.Equal(1m, result.Interest);
        Assert.Equal(101m, result.Payable);
    }

    /// <summary>
    /// Straight-line with half rate in the first year and declining balance.
    /// </summary>
    [Fact]
    public void Depreciation_StraightLineAndDeclining()
    {
        FixedAsset straight = new() { AcquisitionDate = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Unspecified), AcquisitionCost = 10_000m, RatePercent = 20m, HalfRateFirstYear = true };
        var plan = DepreciationCalculator.Plan(straight);

        Assert.Equal(1000m, plan[0].Amount);
        Assert.Equal(2000m, plan[1].Amount);
        Assert.Equal(0m, plan[^1].BookValue);
        Assert.Equal(10_000m, plan.Sum(static p => p.Amount));

        FixedAsset declining = new() { AcquisitionDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), AcquisitionCost = 10_000m, RatePercent = 40m, Method = DepreciationCalculator.Declining };

        Assert.Equal(4000m, DepreciationCalculator.Amount(declining, 2024, 0m));
        Assert.Equal(2400m, DepreciationCalculator.Amount(declining, 2025, 4000m));
        Assert.Equal(10_000m, DepreciationCalculator.Plan(declining).Sum(static p => p.Amount));
    }
}
