// <copyright file="EquipmentHistoryViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the equipment service history view model: the assets with their meters, and the service reports, replaced parts and
/// meter readings of the selected asset, read from the cache first.
/// </summary>
public sealed class EquipmentHistoryViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime SyncedAt = new(2027, 3, 2, 7, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly ILocalizationService loc = LocalizationService.Instance;

    private FieldServiceCached<EquipmentHistory>? history = new(FieldServiceContractParser.ParseHistory(FieldServiceTestData.History), SyncedAt);

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentHistoryViewModelTests"/> class.
    /// </summary>
    public EquipmentHistoryViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetFieldServiceEquipmentQuery>(), Any))
            .ReturnsAsync(new FieldServiceCached<IReadOnlyList<FieldServiceEquipment>>(FieldServiceContractParser.ParseEquipment(FieldServiceTestData.Equipment), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetCustomerEquipmentServiceHistoryQuery>(), Any)).ReturnsAsync(() => history);
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok));
    }

    /// <summary>
    /// Loading selects the first asset and shows its history with the covered parts marked.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheHistoryOfTheFirstAsset()
    {
        var vm = await LoadedAsync();

        Assert.Equal("SN-100", vm.SelectedEquipment?.SerialNumber);
        Assert.True(vm.HasHistory);
        var entry = Assert.Single(vm.Entries);
        Assert.Equal("SR-2027-0001", entry.ReportNumber);
        Assert.Equal("Filter replaced", entry.Summary);
        Assert.Equal(loc["FieldService_Type_preventive"], entry.TypeText);
        Assert.Contains(loc["FieldService_Part_Covered"], entry.PartsText, StringComparison.Ordinal);
        Assert.Contains("x2", entry.PartsText, StringComparison.Ordinal);
        Assert.Contains("SN-100", vm.SummaryText, StringComparison.Ordinal);
        sender.Verify(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// An asset never pulled is pulled once; an unreachable server leaves the history empty and says offline.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_HistoryNotCachedAndNoServer_IsEmptyAndOffline()
    {
        history = null;
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Offline));

        var vm = await LoadedAsync();

        Assert.False(vm.HasHistory);
        Assert.Empty(vm.Entries);
        Assert.True(vm.IsOffline);
        sender.Verify(s => s.Send(It.Is<SyncFieldServiceCommand>(c => c.Scope == FieldServiceSyncScope.History && c.Key == FieldServiceTestData.EquipmentId), Any), Times.Once);
    }

    /// <summary>
    /// Syncing pulls the equipment and the history of the selected asset.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_PullsTheEquipmentAndTheSelectedHistory()
    {
        var vm = await LoadedAsync();

        await vm.SyncAsync();

        sender.Verify(s => s.Send(It.Is<SyncFieldServiceCommand>(c => c.Scope == FieldServiceSyncScope.Equipment), Any), Times.Once);
        sender.Verify(s => s.Send(It.Is<SyncFieldServiceCommand>(c => c.Scope == FieldServiceSyncScope.History && c.Key == FieldServiceTestData.EquipmentId), Any), Times.Once);
        Assert.False(vm.IsOffline);
        Assert.Equal(loc["FieldService_Synced"], vm.StatusMessage);
    }

    private async Task<EquipmentHistoryViewModel> LoadedAsync()
    {
        EquipmentHistoryViewModel vm = new(sender.Object, loc);
        await vm.LoadAsync();

        return vm;
    }
}
