// <copyright file="VatReconciliationTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the reconciliation of the VAT liquidations with the web-api settlements and the Erario c/IVA account, and of the annual
/// reconciliation schedule.
/// </summary>
public sealed class VatReconciliationTests
{
    private static readonly DateTime QuarterStart = new(2027, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime QuarterEnd = new(2027, 3, 31, 0, 0, 0, DateTimeKind.Unspecified);

    /// <summary>
    /// A period whose liquidation, settlement and ledger posting agree is reconciled.
    /// </summary>
    [Fact]
    public void Erario_AgreeingFigures_AreReconciled()
    {
        var result = ErarioReconciler.Reconcile(2027, [Quarter(output: 220m, input: 100m, payable: 120m)], [Settlement(payable: 120m)], accountFound: true, static (_, _) => 120m);

        var row = Assert.Single(result.Rows);
        Assert.Equal(ErarioReconciler.Reconciled, row.State);
        Assert.Equal(120m, row.LocalBalance);
        Assert.Equal(0m, row.Difference);
        Assert.True(result.IsReconciled);
        Assert.Equal(120m, result.TotalLedger);
    }

    /// <summary>
    /// A posting that differs from the liquidation, a missing posting and a missing account are told apart.
    /// </summary>
    [Fact]
    public void Erario_Differences_AreClassified()
    {
        var returns = new[] { Quarter(output: 220m, input: 100m, payable: 120m) };

        var mismatch = ErarioReconciler.Reconcile(2027, returns, [], accountFound: true, static (_, _) => 80m);
        var notPosted = ErarioReconciler.Reconcile(2027, returns, [], accountFound: true, static (_, _) => 0m);
        var noAccount = ErarioReconciler.Reconcile(2027, returns, [], accountFound: false, static (_, _) => 120m);

        Assert.Equal(ErarioReconciler.LedgerMismatch, mismatch.Rows[0].State);
        Assert.Equal(40m, mismatch.Rows[0].Difference);
        Assert.Equal(ErarioReconciler.NotPosted, notPosted.Rows[0].State);
        Assert.Equal(ErarioReconciler.NotPosted, noAccount.Rows[0].State);
        Assert.False(noAccount.IsReconciled);
        Assert.False(noAccount.AccountFound);
    }

    /// <summary>
    /// A settlement of the web-api that differs from the local liquidation is reported before the ledger is looked at.
    /// </summary>
    [Fact]
    public void Erario_ServerDifference_IsReported()
    {
        var result = ErarioReconciler.Reconcile(2027, [Quarter(output: 220m, input: 100m, payable: 120m)], [Settlement(payable: 100m)], accountFound: true, static (_, _) => 120m);

        Assert.Equal(ErarioReconciler.ServerMismatch, result.Rows[0].State);
        Assert.Equal(100m, result.Rows[0].ServerPayable);
    }

    /// <summary>
    /// The ledger reader recognises the VAT payable account by its code and by its name.
    /// </summary>
    [Fact]
    public void ErarioAccount_IsRecognisedByCodeAndName()
    {
        Assert.True(ErarioLedgerReader.IsErarioAccount(new ChartOfAccount { AccountCode = "2200", Name = "x" }));
        Assert.True(ErarioLedgerReader.IsErarioAccount(new ChartOfAccount { Code = "2200", Name = "x" }));
        Assert.True(ErarioLedgerReader.IsErarioAccount(new ChartOfAccount { Code = "175", Name = "Erario c/IVA" }));
        Assert.True(ErarioLedgerReader.IsErarioAccount(new ChartOfAccount { Code = "9", Name = "IVA dovuta" }));
        Assert.False(ErarioLedgerReader.IsErarioAccount(new ChartOfAccount { Code = "1170", Name = "IVA precedente" }));
    }

    /// <summary>
    /// The movements of the ledger are summed per period.
    /// </summary>
    [Fact]
    public void ErarioLedger_SumsTheMovementsBetweenTwoDates()
    {
        ErarioLedger ledger = new(AccountFound: true, [(QuarterStart.AddDays(5), 100m), (QuarterEnd, -30m), (QuarterEnd.AddDays(1), 999m)]);

        Assert.Equal(70m, ledger.Between(QuarterStart, QuarterEnd));
        Assert.Equal(1069m, ledger.Total());
    }

    /// <summary>
    /// The annual schedule totals the periods and finds no differences when liquidations, registers and ledger agree.
    /// </summary>
    [Fact]
    public void Annual_AgreeingFigures_HaveNoFindings()
    {
        var returns = QuarterlyReturns(isFinal: true);
        var register = RegisterFor(returns);

        var annual = AnnualVatReconciler.Build(2027, returns, register, ledgerPosted: returns.Sum(static r => r.OutputVat - r.InputVat));

        Assert.Equal(3, annual.Rows.Count);
        Assert.Equal(660m, annual.TotalOutputVat);
        Assert.Equal(300m, annual.TotalInputVat);
        Assert.Equal(360m, annual.AnnualBalance);
        Assert.Equal(3000m, annual.SalesTaxable);
        Assert.Equal(660m, annual.SalesVat);
        Assert.Equal(300m, annual.PurchasesDeductibleVat);
        Assert.Empty(annual.Findings);
    }

    /// <summary>
    /// Differences against the registers and the ledger, missing periods and draft liquidations are reported.
    /// </summary>
    [Fact]
    public void Annual_Differences_AreReported()
    {
        var returns = QuarterlyReturns(isFinal: false).Take(2).ToList();
        var register = RegisterFor(returns);
        register[0].VatAmount += 10m;
        register[1].DeductibleVat += 5m;

        var annual = AnnualVatReconciler.Build(2027, returns, register, ledgerPosted: 0m);

        Assert.Contains("sales_vat_mismatch", annual.Findings, StringComparer.Ordinal);
        Assert.Contains("purchases_vat_mismatch", annual.Findings, StringComparer.Ordinal);
        Assert.Contains("ledger_mismatch", annual.Findings, StringComparer.Ordinal);
        Assert.Contains("periods_missing", annual.Findings, StringComparer.Ordinal);
        Assert.Contains("periods_not_final", annual.Findings, StringComparer.Ordinal);
    }

    /// <summary>
    /// Split payment and reverse-charge lines are left out of the comparison with the liquidations.
    /// </summary>
    [Fact]
    public void Annual_IgnoresSplitPaymentAndReverseChargeLines()
    {
        var returns = QuarterlyReturns(isFinal: true);
        var register = RegisterFor(returns);
        register.Add(new VatRegisterEntry { Kind = VatRegisterKind.Sales, TaxableAmount = 500m, VatAmount = 110m, IsSplitPayment = true });
        register.Add(new VatRegisterEntry { Kind = VatRegisterKind.Purchases, TaxableAmount = 500m, DeductibleVat = 110m, IsReverseCharge = true });

        var annual = AnnualVatReconciler.Build(2027, returns, register, ledgerPosted: 360m);

        Assert.DoesNotContain("sales_vat_mismatch", annual.Findings, StringComparer.Ordinal);
        Assert.DoesNotContain("purchases_vat_mismatch", annual.Findings, StringComparer.Ordinal);
    }

    private static VatReturn Quarter(decimal output, decimal input, decimal payable, int periodQuarter = 1, bool final = true)
    {
        var from = new DateTime(2027, ((periodQuarter - 1) * 3) + 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

        return new VatReturn
        {
            Country = "IT",
            PeriodFrom = from,
            PeriodTo = from.AddMonths(3).AddDays(-1),
            OutputVat = output,
            InputVat = input,
            Payable = payable,
            Status = final ? "Final" : "Draft",
        };
    }

    private static VatSettlementSnapshot Settlement(decimal payable) => new() { PeriodFrom = QuarterStart, PeriodTo = QuarterEnd, PayableAmount = payable, Year = 2027 };

    private static List<VatReturn> QuarterlyReturns(bool isFinal) =>
    [
        Quarter(220m, 100m, 120m, periodQuarter: 1, isFinal),
        Quarter(220m, 100m, 120m, periodQuarter: 2, isFinal),
        Quarter(220m, 100m, 120m, periodQuarter: 3, isFinal),
    ];

    private static List<VatRegisterEntry> RegisterFor(IReadOnlyList<VatReturn> returns)
    {
        List<VatRegisterEntry> lines = [];
        foreach (var vatReturn in returns)
        {
            lines.Add(new VatRegisterEntry { Kind = VatRegisterKind.Sales, TaxableAmount = 1000m, VatAmount = vatReturn.OutputVat });
            lines.Add(new VatRegisterEntry { Kind = VatRegisterKind.Purchases, TaxableAmount = 500m, DeductibleVat = vatReturn.InputVat });
        }

        return lines;
    }
}
