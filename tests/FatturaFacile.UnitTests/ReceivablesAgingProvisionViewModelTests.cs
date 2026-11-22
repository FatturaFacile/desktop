// <copyright file="ReceivablesAgingProvisionViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using FatturaFacile.Application.Features.Receivables;
using FatturaFacile.Application.Receivables;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Receivables;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the aging and provision view models: buckets rendered from the answer, the drill-down, live provision preview, the rates and the DSO.
/// </summary>
public sealed class ReceivablesAgingProvisionViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly Guid CustomerId = ReceivablesFixtures.CustomerId;

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly List<(string? Key, Guid? Customer)> drills = [];
    private ReceivablesLoad<AgingReport>? aging = ReceivablesFixtures.Load(ReceivablesFixtures.Aging());
    private ReceivablesLoad<DsoResult>? dso = ReceivablesFixtures.Load(new DsoResult(ReceivablesFixtures.Today, 90, "EUR", 1000m, 2000m, 45m));
    private int changed;
    private ReceivablesActionResult result = ReceivablesFixtures.Sent();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivablesAgingProvisionViewModelTests"/> class.
    /// </summary>
    public ReceivablesAgingProvisionViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetReceivablesAgingQuery>(), Any)).ReturnsAsync(() => aging);
        _ = sender.Setup(s => s.Send(It.IsAny<GetBadDebtProvisionQuery>(), Any)).ReturnsAsync(ReceivablesFixtures.Load(ReceivablesFixtures.Provision()));
        _ = sender.Setup(s => s.Send(It.IsAny<GetDsoQuery>(), Any)).ReturnsAsync(() => dso);
        _ = sender.Setup(s => s.Send(It.IsAny<UpdateProvisionRatesCommand>(), Any)).ReturnsAsync(() => result);
    }

    private ReceivablesAgingViewModel CreateAging() => new(sender.Object, loc, (key, customer) =>
    {
        drills.Add((key, customer));

        return Task.CompletedTask;
    });

    private ReceivablesProvisionViewModel CreateProvision() => new(sender.Object, loc, time, () =>
    {
        changed++;

        return Task.CompletedTask;
    });

    private static Guid CustomerIdValue() => CustomerId;

    /// <summary>
    /// The aging renders one cell per bucket of the answer, with the amount and the share, and a row per customer.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Aging_Load_RendersTheBucketsOfTheAnswerWithSharesAndCustomers()
    {
        var vm = CreateAging();
        var culture = loc.CurrentCulture;

        await vm.LoadAsync(null);

        Assert.Equal(["current", "1_30", "31_60", "61_90", "91_120", "121_plus"], vm.Cells.Select(cell => cell.Key));
        Assert.Equal(ReceivablesFormat.Money(600m, culture), vm.Cells[0].AmountText);
        Assert.Equal(ReceivablesFormat.Percent(60m, culture, string.Empty), vm.Cells[0].ShareText);
        Assert.Equal(loc["Receivables_Bucket_Current"], vm.Cells[0].Header);
        Assert.Equal(string.Format(culture, loc["Receivables_Bucket_Range"], "1", "30"), vm.Cells[1].Header);
        Assert.Equal(string.Format(culture, loc["Receivables_Bucket_Open"], "121"), vm.Cells[5].Header);
        var row = Assert.Single(vm.Customers);
        Assert.Equal("Rossi SA", row.Name);
        Assert.Equal(6, row.Cells.Count);
        Assert.Equal(ReceivablesFormat.Money(300m, culture), row.Cells[2].AmountText);
        Assert.Equal(ReceivablesFormat.Money(1000m, culture), vm.TotalOpenText);
        Assert.Equal(ReceivablesFormat.Money(100m, culture), vm.Over90Text);
        Assert.Equal("Rossi SA", vm.CustomerNames[CustomerIdValue()]);
    }

    /// <summary>
    /// A company with its own bounds gets its own buckets and labels: nothing is hardcoded.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Aging_CustomBuckets_AreRenderedFromTheAnswer()
    {
        var engine = new AgingBucketEngine([15, 45]);
        var totals = engine.Buckets.ToDictionary(bucket => bucket.Key, _ => 10m, StringComparer.Ordinal);
        aging = ReceivablesFixtures.Load(new AgingReport(ReceivablesFixtures.Today, "CHF", ["CHF"], engine.Buckets, totals, 40m, 30m, 10m, []));
        var vm = CreateAging();

        await vm.LoadAsync("CHF");

        Assert.Equal(["current", "1_15", "16_45", "46_plus"], vm.Cells.Select(cell => cell.Key));
        Assert.Equal(string.Format(loc.CurrentCulture, loc["Receivables_Bucket_Range"], "16", "45"), vm.Cells[2].Header);
        Assert.Equal(4, vm.Buckets.Count);
        Assert.Equal("CHF", vm.CurrencyText);
    }

    /// <summary>
    /// Without data the aging says so and shows nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Aging_NoData_ShowsTheMessage()
    {
        aging = null;
        var vm = CreateAging();

        await vm.LoadAsync(null);

        Assert.Empty(vm.Cells);
        Assert.Equal(loc["Receivables_NoData"], vm.StatusMessage);
    }

    /// <summary>
    /// A cached aging shows the offline notice.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Aging_Offline_ShowsTheNotice()
    {
        aging = aging! with { FromCache = true };
        var vm = CreateAging();

        await vm.LoadAsync(null);

        Assert.Equal(loc["Receivables_OfflineCache"], vm.NoticeText);
    }

    /// <summary>
    /// Clicking a figure hands the bucket and the customer to the drill-down callback; a missing cell does nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Aging_DrillDown_CallsBackWithTheBucketAndTheCustomer()
    {
        var vm = CreateAging();
        await vm.LoadAsync(null);

        await vm.DrillDownCommand.ExecuteAsync(vm.Customers[0].Cells[2]);
        await vm.DrillDownCommand.ExecuteAsync(vm.Cells[5]);
        await vm.DrillDownCommand.ExecuteAsync(null);

        Assert.Equal(2, drills.Count);
        Assert.Equal(("31_60", (Guid?)CustomerIdValue()), drills[0]);
        Assert.Equal(("121_plus", (Guid?)null), drills[1]);
    }

    /// <summary>
    /// The provision shows a row per bucket with its default rate, the totals and the DSO.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Provision_Load_ShowsTheRowsTheTotalsAndTheDso()
    {
        var vm = CreateProvision();
        var culture = loc.CurrentCulture;

        await vm.LoadAsync(null);

        Assert.Equal(6, vm.Rows.Count);
        Assert.Equal(0.5m.ToString("0.##", culture), vm.Rows[0].RateText);
        Assert.Equal(ReceivablesFormat.Money(5m, culture), vm.Rows[0].ProvisionText);
        Assert.True(vm.Rows[0].IsDefault);
        Assert.Equal(ReceivablesFormat.Money(6000m, culture), vm.TotalOpenText);
        Assert.Equal(ReceivablesFormat.Money(915m, culture), vm.TotalProvisionText);
        Assert.Equal(ReceivablesFormat.Money(5085m, culture), vm.NetRealizableText);
        Assert.Equal(string.Format(culture, loc["Receivables_DsoDays"], 45m.ToString("0.##", culture)), vm.DsoText);
        sender.Verify(s => s.Send(It.Is<GetDsoQuery>(q => q.PeriodDays == 90), Any), Times.Once);
    }

    /// <summary>
    /// A DSO that is undefined (receivables but no sales) is shown as such, never as zero.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Provision_DsoUndefined_ShowsTheUndefinedText()
    {
        dso = ReceivablesFixtures.Load(new DsoResult(ReceivablesFixtures.Today, 90, "EUR", 1000m, 0m, null));
        var vm = CreateProvision();

        await vm.LoadAsync(null);

        Assert.Equal(loc["Receivables_DsoUndefined"], vm.DsoText);
    }

    /// <summary>
    /// The DSO period typed by the user is sent; text that is not a number falls back to 90 days.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Provision_PeriodDays_AreSentAndBadTextFallsBackToNinety()
    {
        var vm = CreateProvision();
        vm.PeriodDaysText = "30";

        await vm.LoadAsync(null);
        vm.PeriodDaysText = "abc";
        await vm.LoadAsync(null);

        sender.Verify(s => s.Send(It.Is<GetDsoQuery>(q => q.PeriodDays == 30), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<GetDsoQuery>(q => q.PeriodDays == 90), Any), Times.Once);
    }

    /// <summary>
    /// Editing a rate recalculates the line, the total and the net realizable value with the domain calculator.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Provision_EditRate_RecalculatesTheProvisionLive()
    {
        var vm = CreateProvision();
        await vm.LoadAsync(null);
        var culture = loc.CurrentCulture;

        vm.Rows[3].RateText = "20";

        Assert.Equal(ReceivablesFormat.Money(200m, culture), vm.Rows[3].ProvisionText);
        Assert.False(vm.Rows[3].IsDefault);
        Assert.Equal(ReceivablesFormat.Money(1015m, culture), vm.TotalProvisionText);
        Assert.Equal(ReceivablesFormat.Money(4985m, culture), vm.NetRealizableText);
    }

    /// <summary>
    /// A rate outside 0 to 100 shows the error and leaves the totals as they were.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Provision_InvalidRate_ShowsTheErrorAndKeepsTheTotals()
    {
        var vm = CreateProvision();
        await vm.LoadAsync(null);
        var total = vm.TotalProvisionText;

        vm.Rows[1].RateText = "150";

        Assert.Equal(loc[ReceivablesService.RateKey], vm.StatusMessage);
        Assert.Equal(total, vm.TotalProvisionText);
    }

    /// <summary>
    /// Resetting restores the default rates and the default flag.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Provision_ResetDefaults_RestoresTheDefaultRates()
    {
        var vm = CreateProvision();
        await vm.LoadAsync(null);
        vm.Rows[3].RateText = "20";

        vm.ResetDefaultsCommand.Execute(null);

        Assert.Equal(10m.ToString("0.##", loc.CurrentCulture), vm.Rows[3].RateText);
        Assert.True(vm.Rows[3].IsDefault);
        Assert.Equal(ReceivablesFormat.Money(915m, loc.CurrentCulture), vm.TotalProvisionText);
    }

    /// <summary>
    /// Saving sends the percentage of every bucket, shows the outcome and refreshes the dashboard.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Provision_SaveRates_SendsTheRateOfEveryBucket()
    {
        var vm = CreateProvision();
        await vm.LoadAsync(null);
        vm.Rows[3].RateText = "12.5";

        await vm.SaveRatesCommand.ExecuteAsync(null);

        sender.Verify(s => s.Send(It.Is<UpdateProvisionRatesCommand>(c => c.Request.Rates.Count == 6 && c.Request.Rates["current"] == 0.5m && c.Request.Rates["61_90"] == 12.5m && c.Request.Rates["121_plus"] == 50m), Any), Times.Once);
        Assert.Equal(loc["Receivables_Sent"], vm.ResultText);
        Assert.Equal(1, changed);
    }

    /// <summary>
    /// Invalid rates are never sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Provision_SaveRates_Invalid_SendsNothing()
    {
        var vm = CreateProvision();
        await vm.LoadAsync(null);
        vm.Rows[0].RateText = "abc";

        await vm.SaveRatesCommand.ExecuteAsync(null);

        Assert.Equal(loc[ReceivablesService.RateKey], vm.ResultText);
        sender.Verify(s => s.Send(It.IsAny<UpdateProvisionRatesCommand>(), Any), Times.Never);
    }
}
