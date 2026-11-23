// <copyright file="BlanketBalanceViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.SupplierRebates;
using FatturaFacile.Application.SupplierRebates;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the blanket agreement balance view model: figures and alerts from the cache, the first pull, the offline and error states.
/// </summary>
public sealed class BlanketBalanceViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime Synced = new(2027, 4, 1, 8, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly Mock<ILocalizationService> localization = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BlanketBalanceViewModelTests"/> class.
    /// </summary>
    public BlanketBalanceViewModelTests()
    {
        _ = localization.Setup(l => l[It.IsAny<string>()]).Returns((string key) => "[" + key + "]");
        _ = localization.SetupGet(l => l.CurrentCulture).Returns(CultureInfo.InvariantCulture);
        _ = sender.Setup(s => s.Send(It.IsAny<GetBlanketContractsQuery>(), Any)).ReturnsAsync(Contracts(Synced));
        _ = sender.Setup(s => s.Send(It.IsAny<GetBlanketBalanceQuery>(), Any)).ReturnsAsync(new RebateCached<BlanketBalance>(SupplierRebatesTestData.Balance(), Synced));
        _ = sender.Setup(s => s.Send(It.IsAny<SyncSupplierRebatesCommand>(), Any)).ReturnsAsync(new SupplierRebatesResult(SupplierRebatesStatus.Synced, Synced));
    }

    /// <summary>
    /// The balance shows the ceiling, the minimum, the percentages, the items and the alerts of the selected agreement.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_FromCache_ShowsTheFiguresTheItemsAndTheAlerts()
    {
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        Assert.Equal(SupplierRebatesTestData.ContractId, viewModel.SelectedContract?.Id);
        Assert.Equal("RQ-2027-01 - Widgets", viewModel.SelectedContract?.Label);
        Assert.True(viewModel.HasBalance);
        Assert.False(viewModel.IsBusy);
        Assert.Equal("CHF 100,000.00", viewModel.CeilingText);
        Assert.Equal("CHF 60,000.00", viewModel.MinimumText);
        Assert.Equal("CHF 85,000.00", viewModel.OrderedText);
        Assert.Equal("CHF 15,000.00", viewModel.RemainingText);
        Assert.Equal("CHF 0.00", viewModel.ShortfallText);
        Assert.Equal("85.00%", viewModel.UsedText);
        Assert.Equal(85d, viewModel.UsedPercent);
        Assert.Equal(100d, viewModel.ReachedPercent);
        Assert.Equal("141.67%", viewModel.ReachedText);
        Assert.True(viewModel.HasAlerts);
        var alert = Assert.Single(viewModel.Alerts);
        Assert.Equal("[SupplierRebates_AlertCeilingNear]", alert.Text);
        Assert.Equal("warning", alert.Severity);
        Assert.Equal("[SupplierRebates_SeverityWarning]", alert.SeverityText);
        var line = Assert.Single(viewModel.Lines);
        Assert.Equal("Widgets", line.Description);
        Assert.Equal("3200", line.Remaining);
        Assert.Equal("CHF 12.50", line.UnitPrice);
    }

    /// <summary>
    /// Every alert code of the server has its own text and a critical one has its own severity.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_AlertCodes_AreTranslated()
    {
        var balance = SupplierRebatesTestData.Balance() with
        {
            Alerts =
            [
                new BlanketAlert(RebateCodes.AlertCeilingExceeded, "critical"),
                new BlanketAlert(RebateCodes.AlertMinimumAtRisk, "warning"),
                new BlanketAlert(RebateCodes.AlertMinimumNotMet, "critical"),
                new BlanketAlert("something_new", "warning"),
            ],
        };
        _ = sender.Setup(s => s.Send(It.IsAny<GetBlanketBalanceQuery>(), Any)).ReturnsAsync(new RebateCached<BlanketBalance>(balance, Synced));
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        string[] expected = ["[SupplierRebates_AlertCeilingExceeded]", "[SupplierRebates_AlertMinimumAtRisk]", "[SupplierRebates_AlertMinimumNotMet]", "something_new"];
        Assert.Equal(expected, viewModel.Alerts.Select(alert => alert.Text).ToArray());
        Assert.Equal("[SupplierRebates_SeverityCritical]", viewModel.Alerts[0].SeverityText);
        Assert.Equal("[SupplierRebates_SeverityWarning]", viewModel.Alerts[1].SeverityText);
    }

    /// <summary>
    /// A balance without alerts, minimum or ceiling shows dashes and no alert.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_WithoutMinimumAndAlerts_ShowsDashes()
    {
        var balance = SupplierRebatesTestData.Balance() with
        {
            MinimumSpendAmount = null,
            RemainingCeiling = null,
            CeilingUsedPercent = null,
            MinimumShortfall = null,
            MinimumReachedPercent = null,
            Alerts = [],
        };
        _ = sender.Setup(s => s.Send(It.IsAny<GetBlanketBalanceQuery>(), Any)).ReturnsAsync(new RebateCached<BlanketBalance>(balance, Synced));
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        Assert.False(viewModel.HasAlerts);
        Assert.Empty(viewModel.Alerts);
        Assert.Equal("-", viewModel.MinimumText);
        Assert.Equal("-", viewModel.RemainingText);
        Assert.Equal("-", viewModel.UsedText);
        Assert.Equal(0d, viewModel.UsedPercent);
    }

    /// <summary>
    /// When nothing was ever pulled the view pulls the blanket agreements first, then the balance of the selected one.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_NeverSynced_PullsTheAgreementsFirst()
    {
        _ = sender.SetupSequence(s => s.Send(It.IsAny<GetBlanketContractsQuery>(), Any))
            .ReturnsAsync(Contracts(syncedAt: null))
            .ReturnsAsync(Contracts(Synced));
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<SyncSupplierRebatesCommand>(c => c.Scope == RebateSyncScope.BlanketContracts), Any), Times.Once);
        _ = Assert.Single(viewModel.Contracts);
        Assert.True(viewModel.HasBalance);
    }

    /// <summary>
    /// A balance that was never pulled is pulled when the agreement is selected, and shows the notice when it cannot be.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_MissingBalance_IsPulledAndShowsTheNoticeWhenItStaysMissing()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetBlanketBalanceQuery>(), Any)).ReturnsAsync((RebateCached<BlanketBalance>?)null);
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        sender.Verify(s => s.Send(It.Is<SyncSupplierRebatesCommand>(c => c.Scope == RebateSyncScope.BlanketBalance && c.Id == SupplierRebatesTestData.ContractId), Any), Times.Once);
        Assert.False(viewModel.HasBalance);
        Assert.Equal("-", viewModel.CeilingText);
        Assert.Equal("[SupplierRebates_Synced]", viewModel.StatusMessage);
    }

    /// <summary>
    /// Without a connection the sync keeps the cached balance on screen and says so.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Offline_KeepsTheCachedBalanceAndFlagsOffline()
    {
        var viewModel = Create();
        await viewModel.LoadCommand.ExecuteAsync(parameter: null);
        _ = sender.Setup(s => s.Send(It.IsAny<SyncSupplierRebatesCommand>(), Any)).ReturnsAsync(new SupplierRebatesResult(SupplierRebatesStatus.Offline, Detail: "down"));

        await viewModel.SyncCommand.ExecuteAsync(parameter: null);

        Assert.True(viewModel.IsOffline);
        Assert.Equal("[SupplierRebates_OfflineUsingCache]", viewModel.StatusMessage);
        Assert.True(viewModel.HasBalance);
        Assert.Equal("CHF 100,000.00", viewModel.CeilingText);
        Assert.False(viewModel.IsBusy);
    }

    /// <summary>
    /// A refused permission shows its message and the view stops loading; the sync command is blocked while the view is busy.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_AccessDenied_ShowsTheMessageAndSyncIsBlockedWhileBusy()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetBlanketContractsQuery>(), Any)).ThrowsAsync(new UnauthorizedAccessException("denied"));
        var viewModel = Create();

        await viewModel.LoadCommand.ExecuteAsync(parameter: null);

        Assert.Equal("denied", viewModel.StatusMessage);
        Assert.False(viewModel.IsBusy);
        Assert.True(viewModel.SyncCommand.CanExecute(parameter: null));

        TaskCompletionSource<SupplierRebatesResult> gate = new();
        _ = sender.Setup(s => s.Send(It.IsAny<GetBlanketContractsQuery>(), Any)).ReturnsAsync(Contracts(Synced));
        _ = sender.Setup(s => s.Send(It.IsAny<SyncSupplierRebatesCommand>(), Any)).Returns(gate.Task);
        var running = viewModel.SyncCommand.ExecuteAsync(parameter: null);

        Assert.True(viewModel.IsBusy);
        Assert.False(viewModel.SyncCommand.CanExecute(parameter: null));

        gate.SetResult(new SupplierRebatesResult(SupplierRebatesStatus.Failed, Detail: "HTTP 500"));
        await running;

        Assert.False(viewModel.IsBusy);
        Assert.Equal("[SupplierRebates_SyncFailed]", viewModel.StatusMessage);
    }

    private static RebateCached<IReadOnlyList<BlanketContract>> Contracts(DateTime? syncedAt) =>
        syncedAt is null
            ? new RebateCached<IReadOnlyList<BlanketContract>>([], SyncedAtUtc: null)
            : new RebateCached<IReadOnlyList<BlanketContract>>(SupplierRebateParser.ParseBlanketContracts(SupplierRebatesTestData.ContractsJson), syncedAt);

    private BlanketBalanceViewModel Create() => new(sender.Object, localization.Object);
}
