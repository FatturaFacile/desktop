// <copyright file="MaintenanceContractsViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.FieldService;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the maintenance contracts view model: the list, the editor with coverage type, preventive frequency, included visits
/// and call-out fee, and the save of a new or an edited contract.
/// </summary>
public sealed class MaintenanceContractsViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();
    private static readonly DateTime SyncedAt = new(2027, 3, 2, 7, 0, 0, DateTimeKind.Utc);

    private readonly Mock<ISender> sender = new();
    private readonly ILocalizationService loc = LocalizationService.Instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaintenanceContractsViewModelTests"/> class.
    /// </summary>
    public MaintenanceContractsViewModelTests()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetMaintenanceContractsQuery>(), Any))
            .ReturnsAsync(new FieldServiceCached<IReadOnlyList<MaintenanceContract>>(FieldServiceContractParser.ParseContracts(FieldServiceTestData.Contracts), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<GetFieldServiceEquipmentQuery>(), Any))
            .ReturnsAsync(new FieldServiceCached<IReadOnlyList<FieldServiceEquipment>>(FieldServiceContractParser.ParseEquipment(FieldServiceTestData.Equipment), SyncedAt));
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok));
    }

    /// <summary>
    /// Loading lists the contracts with their coverage, visits and call-out fee, and offers all four coverage types.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ListsTheContractsAndTheCoverageChoices()
    {
        var vm = await LoadedAsync();

        Assert.Equal(2, vm.Contracts.Count);
        Assert.Equal("MC-001", vm.Contracts[0].Number);
        Assert.Equal(loc["FieldService_Coverage_PartsAndLabor"], vm.Contracts[0].CoverageText);
        Assert.Equal(loc["FieldService_ContractStatus_active"], vm.Contracts[0].StatusText);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["FieldService_VisitsOf"], "1", "4"), vm.Contracts[0].VisitsText);
        Assert.Contains("50", vm.Contracts[0].CalloutText, StringComparison.Ordinal);
        Assert.Equal(
            [MaintenanceCoverageType.LaborOnly, MaintenanceCoverageType.PartsAndLabor, MaintenanceCoverageType.PreventiveOnly, MaintenanceCoverageType.FullComprehensive],
            vm.Coverages.Select(choice => choice.Value));
        Assert.Equal("-", vm.Contracts[1].FrequencyText);
        Assert.True(vm.IsNew);
        _ = Assert.Single(vm.Equipment);
    }

    /// <summary>
    /// Selecting a contract fills the editor with its coverage, preventive rule, visits, fees and covered equipment.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task SelectingAContract_FillsTheEditor()
    {
        var vm = await LoadedAsync();

        vm.SelectedContract = vm.Contracts[0];

        Assert.False(vm.IsNew);
        Assert.Equal("MC-001", vm.EditNumber);
        Assert.Equal("Chiller plan", vm.EditTitle);
        Assert.Equal("cust-1", vm.EditCustomerId);
        Assert.Equal(new DateTime(2027, 1, 1), vm.EditStartDate);
        Assert.Equal(new DateTime(2027, 12, 31), vm.EditEndDate);
        Assert.Equal("active", vm.SelectedStatus?.Value);
        Assert.Equal(MaintenanceCoverageType.PartsAndLabor, vm.SelectedCoverage?.Value);
        Assert.Equal("6", vm.CalendarEvery);
        Assert.Equal("month", vm.SelectedUnit?.Value);
        Assert.Equal("500", vm.OperatingHours);
        Assert.Equal("10000", vm.Cycles);
        Assert.Equal("10", vm.LeadPercent);
        Assert.Equal("14", vm.LeadDays);
        Assert.Equal("4", vm.IncludedVisits);
        Assert.Equal("1", vm.UsedVisitsText);
        Assert.Equal("50", vm.CalloutFee);
        Assert.Equal("80", vm.HourlyRate);
        Assert.Equal("CHF", vm.Currency);
        Assert.True(vm.Equipment[0].IsSelected);

        vm.NewContract();

        Assert.True(vm.IsNew);
        Assert.Empty(vm.EditNumber);
        Assert.Null(vm.SelectedContract);
        Assert.False(vm.Equipment[0].IsSelected);
        Assert.Empty(vm.CalendarEvery);
    }

    /// <summary>
    /// Saving a new contract sends the editor fields with no identifier and then refreshes the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_New_SendsTheEditorFieldsAndRefreshes()
    {
        FieldServiceContractInput? sent = null;
        _ = sender.Setup(s => s.Send(It.IsAny<SaveMaintenanceContractCommand>(), Any))
            .Callback((IRequest<FieldServiceCallResult> command, CancellationToken _) => sent = ((SaveMaintenanceContractCommand)command).Contract)
            .ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok, Id: "new-id"));
        var vm = await LoadedAsync();
        vm.EditNumber = " MC-100 ";
        vm.EditTitle = "Boiler plan";
        vm.EditCustomerId = FieldServiceTestData.ContractId;
        vm.EditStartDate = new DateTime(2027, 5, 1);
        vm.EditEndDate = new DateTime(2028, 4, 30);
        vm.SelectedCoverage = vm.Coverages.Single(choice => choice.Value == MaintenanceCoverageType.PreventiveOnly);
        vm.CalendarEvery = "3";
        vm.SelectedUnit = vm.Units.Single(choice => string.Equals(choice.Value, "week", StringComparison.Ordinal));
        vm.OperatingHours = "250.5";
        vm.IncludedVisits = "6";
        vm.CalloutFee = "49.9";
        vm.HourlyRate = string.Empty;
        vm.Currency = "chf";
        vm.Equipment[0].IsSelected = true;

        await vm.SaveAsync();

        Assert.NotNull(sent);
        Assert.Null(sent.Id);
        Assert.Equal("MC-100", sent.Number);
        Assert.Equal(new DateOnly(2027, 5, 1), sent.StartDate);
        Assert.Equal(new DateOnly(2028, 4, 30), sent.EndDate);
        Assert.Equal(MaintenanceCoverageType.PreventiveOnly, sent.Coverage);
        Assert.Equal(6, sent.IncludedVisits);
        Assert.Equal(49.9m, sent.CalloutFee);
        Assert.Null(sent.HourlyRate);
        Assert.Equal("CHF", sent.Currency);
        Assert.NotNull(sent.Frequency);
        Assert.Equal(3, sent.Frequency.CalendarEvery);
        Assert.Equal("week", sent.Frequency.CalendarUnit);
        Assert.Equal(250.5m, sent.Frequency.OperatingHours);
        Assert.Null(sent.Frequency.Cycles);
        Assert.Equal([FieldServiceTestData.EquipmentId], sent.EquipmentIds);
        Assert.Equal(loc["FieldService_ContractSaved"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.Is<SyncFieldServiceCommand>(c => c.Scope == FieldServiceSyncScope.Contracts), Any), Times.Once);
    }

    /// <summary>
    /// Saving an edited contract sends its identifier, and a contract without any frequency field sends no preventive rule.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_Edit_SendsTheIdentifierAndNoRuleWhenBlank()
    {
        FieldServiceContractInput? sent = null;
        _ = sender.Setup(s => s.Send(It.IsAny<SaveMaintenanceContractCommand>(), Any))
            .Callback((IRequest<FieldServiceCallResult> command, CancellationToken _) => sent = ((SaveMaintenanceContractCommand)command).Contract)
            .ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Ok, Id: FieldServiceTestData.ContractId));
        var vm = await LoadedAsync();
        vm.SelectedContract = vm.Contracts[0];
        vm.CalendarEvery = string.Empty;
        vm.OperatingHours = string.Empty;
        vm.Cycles = string.Empty;
        vm.LeadDays = string.Empty;
        vm.LeadPercent = string.Empty;

        await vm.SaveAsync();

        Assert.NotNull(sent);
        Assert.Equal(FieldServiceTestData.ContractId, sent.Id);
        Assert.Null(sent.Frequency);
    }

    /// <summary>
    /// A number that is not a number is refused before anything is sent.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_WithABadNumber_IsRefusedLocally()
    {
        var vm = await LoadedAsync();
        vm.CalloutFee = "fifty";

        await vm.SaveAsync();

        Assert.Equal(loc["FieldService_Err_Number"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<SaveMaintenanceContractCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A contract the server refuses shows the server reason and does not refresh the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_Refused_ShowsTheReason()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<SaveMaintenanceContractCommand>(), Any))
            .ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Rejected, "contract_number: Already taken"));
        var vm = await LoadedAsync();

        await vm.SaveAsync();

        Assert.Contains("Already taken", vm.StatusMessage, StringComparison.Ordinal);
        sender.Verify(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A validation failure of the pipeline is shown with its localized message.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Save_ValidationFailure_ShowsTheLocalizedMessage()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<SaveMaintenanceContractCommand>(), Any))
            .ThrowsAsync(new ValidationException([new ValidationFailure("Contract.Currency", "FieldService_Err_Currency")]));
        var vm = await LoadedAsync();

        await vm.SaveAsync();

        Assert.Equal(loc["FieldService_Err_Currency"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// Syncing pulls the equipment and the contracts, and an unreachable server keeps the cache on screen.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Sync_Offline_KeepsTheCacheOnScreen()
    {
        var vm = await LoadedAsync();
        _ = sender.Setup(s => s.Send(It.IsAny<SyncFieldServiceCommand>(), Any)).ReturnsAsync(new FieldServiceCallResult(FieldServiceCallStatus.Offline));

        await vm.SyncAsync();

        Assert.True(vm.IsOffline);
        Assert.Equal(2, vm.Contracts.Count);
        Assert.Equal(loc["FieldService_OfflineUsingCache"], vm.StatusMessage);
    }

    private async Task<MaintenanceContractsViewModel> LoadedAsync()
    {
        MaintenanceContractsViewModel vm = new(sender.Object, loc);
        await vm.LoadAsync();

        return vm;
    }
}
