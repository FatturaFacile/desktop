// <copyright file="LocalHolidaysViewModelTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0003 // Name the parameter: positional records with literal arguments read better unnamed in tests.
using System.Net;
using System.Net.Http;
using FatturaFacile.Application.Features.ResourcePlanning;
using FatturaFacile.Application.ResourcePlanning;
using FatturaFacile.Wpf.Localization;
using FatturaFacile.Wpf.ViewModels;
using MediatR;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the holidays view model: the local holidays of the tenant (list, add, remove with confirmation) and the resolved holidays
/// of a place and a year.
/// </summary>
public sealed class LocalHolidaysViewModelTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private readonly Mock<ISender> sender = new();
    private readonly LocalizationService loc = LocalizationService.Instance;
    private readonly ManualTimeProvider time = new();
    private readonly LocalHoliday fair = new(Guid.NewGuid(), "CH", "TI", "Lugano", 8, 15, null, "Local fair", 0.5m);
    private readonly LocalHoliday closure = new(Guid.NewGuid(), "IT", null, null, 6, 2, 2027, "Plant closure", 1m);
    private IReadOnlyList<LocalHoliday> holidays;
    private IReadOnlyList<PlanningHoliday> resolved;
    private PlanningCommandOutcome<LocalHoliday> added;
    private PlanningCommandOutcome<bool> removed = new(HttpStatusCode.NoContent, true, []);

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalHolidaysViewModelTests"/> class.
    /// </summary>
    public LocalHolidaysViewModelTests()
    {
        holidays = [closure, fair];
        resolved =
        [
            new PlanningHoliday(new DateOnly(2026, 12, 25), "Christmas", 1m),
            new PlanningHoliday(new DateOnly(2026, 1, 1), "New Year", 1m),
            new PlanningHoliday(new DateOnly(2026, 8, 15), "Local fair", 0.5m),
        ];
        added = new PlanningCommandOutcome<LocalHoliday>(HttpStatusCode.Created, fair, []);
        _ = sender.Setup(s => s.Send(It.IsAny<GetLocalHolidaysQuery>(), Any)).ReturnsAsync(() => holidays);
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningHolidaysQuery>(), Any)).ReturnsAsync(() => resolved);
        _ = sender.Setup(s => s.Send(It.IsAny<CreateLocalHolidayCommand>(), Any)).ReturnsAsync(() => added);
        _ = sender.Setup(s => s.Send(It.IsAny<DeleteLocalHolidayCommand>(), Any)).ReturnsAsync(() => removed);
    }

    /// <summary>
    /// The list shows the local holidays by day of the year with their place, date and fraction of the day.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_ShowsTheLocalHolidays()
    {
        var vm = Create();

        await vm.LoadAsync();

        Assert.True(vm.HasData);
        Assert.Equal(new[] { "Plant closure", "Local fair" }, vm.Rows.Select(row => row.Holiday.Name));
        Assert.Equal("IT", vm.Rows[0].ScopeText);
        Assert.Equal("CH / TI / Lugano", vm.Rows[1].ScopeText);
        Assert.Equal(new DateOnly(2027, 6, 2).ToString("d", loc.CurrentCulture), vm.Rows[0].DateText);
        Assert.Equal(new DateOnly(2024, 8, 15).ToString("m", loc.CurrentCulture), vm.Rows[1].DateText);
        Assert.Equal(0.5m.ToString("0.##", loc.CurrentCulture), vm.Rows[1].FractionText);
        Assert.Equal(new[] { "CH", "IT" }, vm.CountryChoices.Select(choice => choice.Key));
    }

    /// <summary>
    /// Without local holidays the list says so; a list that cannot be loaded shows the load failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Load_EmptyAndServerDown_AreExplained()
    {
        holidays = [];
        var vm = Create();

        await vm.LoadAsync();
        Assert.False(vm.HasData);
        Assert.Equal(loc["ResourcePlanning_Holidays_Empty"], vm.StatusMessage);

        _ = sender.Setup(s => s.Send(It.IsAny<GetLocalHolidaysQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        await vm.LoadAsync();

        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// A complete form adds the holiday, normalized, reloads the list and clears what describes the holiday.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Add_SendsTheFormAndClearsIt()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedCountry = vm.CountryChoices[0];
        vm.RegionText = " ti ";
        vm.MunicipalityText = "Lugano";
        vm.NameText = " Local fair ";
        vm.DayText = "15";
        vm.MonthText = "8";
        vm.FractionText = 0.5m.ToString(loc.CurrentCulture);

        await vm.AddAsync();

        sender.Verify(s => s.Send(It.Is<CreateLocalHolidayCommand>(c => c.Holiday == new LocalHolidayRequest("CH", "TI", "Lugano", 8, 15, null, "Local fair", 0.5m)), Any), Times.Once);
        sender.Verify(s => s.Send(It.IsAny<GetLocalHolidaysQuery>(), Any), Times.Exactly(2));
        Assert.Equal(loc["ResourcePlanning_Holidays_Added"], vm.StatusMessage);
        Assert.Equal(string.Empty, vm.NameText);
        Assert.Equal(string.Empty, vm.DayText);
        Assert.Equal("1", vm.FractionText);
    }

    /// <summary>
    /// A holiday that is only for one year sends the year.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Add_WithAYear_SendsTheYear()
    {
        var vm = Create();
        vm.SelectedCountry = vm.CountryChoices[1];
        vm.NameText = "Plant closure";
        vm.DayText = "2";
        vm.MonthText = "6";
        vm.YearText = "2027";

        await vm.AddAsync();

        sender.Verify(s => s.Send(It.Is<CreateLocalHolidayCommand>(c => c.Holiday.Country == "IT" && c.Holiday.Year == 2027 && c.Holiday.RegionCode == null && c.Holiday.DayFraction == 1m), Any), Times.Once);
    }

    /// <summary>
    /// A form that is not valid is explained and not sent.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="day">The day.</param>
    /// <param name="month">The month.</param>
    /// <param name="year">The year.</param>
    /// <param name="fraction">The fraction of the day.</param>
    /// <param name="expected">The expected problem code.</param>
    /// <returns>A task representing test execution.</returns>
    [Theory]
    [InlineData("", "15", "8", "", "1", PlanningDraftBuilder.HolidayNeedsName)]
    [InlineData("Fair", "", "8", "", "1", PlanningDraftBuilder.HolidayInvalidDate)]
    [InlineData("Fair", "31", "4", "", "1", PlanningDraftBuilder.HolidayInvalidDate)]
    [InlineData("Fair", "15", "8", "abc", "1", PlanningDraftBuilder.HolidayInvalidDate)]
    [InlineData("Fair", "15", "8", "", "0", PlanningDraftBuilder.HolidayInvalidFraction)]
    [InlineData("Fair", "15", "8", "", "abc", PlanningDraftBuilder.HolidayInvalidFraction)]
    public async Task Add_InvalidForm_IsExplainedAndNotSent(string name, string day, string month, string year, string fraction, string expected)
    {
        var vm = Create();
        vm.NameText = name;
        vm.DayText = day;
        vm.MonthText = month;
        vm.YearText = year;
        vm.FractionText = fraction;

        await vm.AddAsync();

        Assert.Equal(loc["ResourcePlanning_Setup_" + expected], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<CreateLocalHolidayCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A refusal of the server is shown with the general text and the form is kept; an unreachable server shows the save failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Add_RefusedAndServerDown_AreExplained()
    {
        added = new PlanningCommandOutcome<LocalHoliday>(HttpStatusCode.Forbidden, null, []);
        var vm = Create();
        vm.NameText = "Fair";
        vm.DayText = "15";
        vm.MonthText = "8";

        await vm.AddAsync();
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Error_Generic"], "403"), vm.StatusMessage);
        Assert.Equal("Fair", vm.NameText);

        _ = sender.Setup(s => s.Send(It.IsAny<CreateLocalHolidayCommand>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        await vm.AddAsync();

        Assert.Equal(loc["ResourcePlanning_SaveFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// Removing asks for a confirmation first; confirming removes the holiday and reloads the list.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Remove_NeedsAConfirmation()
    {
        var vm = Create();
        await vm.LoadAsync();
        vm.RequestDelete();
        Assert.Equal(loc["ResourcePlanning_Holidays_NeedsSelection"], vm.StatusMessage);

        vm.SelectedRow = vm.Rows.Single(row => row.Holiday.Id == fair.Id);
        vm.RequestDelete();

        Assert.True(vm.IsDeletePending);
        Assert.Equal(string.Format(loc.CurrentCulture, loc["ResourcePlanning_Holidays_DeleteConfirm"], "Local fair", vm.SelectedRow.DateText), vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<DeleteLocalHolidayCommand>(), Any), Times.Never);

        holidays = [closure];
        await vm.ConfirmDeleteAsync();

        sender.Verify(s => s.Send(It.Is<DeleteLocalHolidayCommand>(c => c.HolidayId == fair.Id), Any), Times.Once);
        Assert.Single(vm.Rows);
        Assert.False(vm.IsDeletePending);
        Assert.Equal(loc["ResourcePlanning_Holidays_Removed"], vm.StatusMessage);
    }

    /// <summary>
    /// Cancelling, selecting another holiday, or confirming without a request removes nothing.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Remove_CancelledOrUnrequested_RemovesNothing()
    {
        var vm = Create();
        await vm.LoadAsync();
        await vm.ConfirmDeleteAsync();
        vm.SelectedRow = vm.Rows[0];
        vm.RequestDelete();
        vm.CancelDelete();
        await vm.ConfirmDeleteAsync();
        vm.RequestDelete();
        vm.SelectedRow = vm.Rows[1];
        await vm.ConfirmDeleteAsync();

        Assert.False(vm.IsDeletePending);
        sender.Verify(s => s.Send(It.IsAny<DeleteLocalHolidayCommand>(), Any), Times.Never);
    }

    /// <summary>
    /// A refused removal shows the violation of the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Remove_Refused_ShowsTheViolation()
    {
        removed = new PlanningCommandOutcome<bool>(HttpStatusCode.NotFound, false, [new PlanningIssue("local_holiday_not_found", null)]);
        var vm = Create();
        await vm.LoadAsync();
        vm.SelectedRow = vm.Rows[0];
        vm.RequestDelete();

        await vm.ConfirmDeleteAsync();

        Assert.Equal(loc["ResourcePlanning_Error_local_holiday_not_found"], vm.StatusMessage);
    }

    /// <summary>
    /// The resolved holidays of a place are asked for with the country, the region in upper case, the municipality and the year, and shown by date.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ShowResolved_AsksForThePlaceAndTheYear()
    {
        var vm = Create();
        Assert.Equal(2026.ToString(loc.CurrentCulture), vm.ResolveYearText);
        vm.ResolveCountry = vm.CountryChoices[0];
        vm.ResolveRegionText = " ti ";
        vm.ResolveMunicipalityText = " Lugano ";
        vm.ResolveYearText = 2027.ToString(loc.CurrentCulture);

        await vm.ShowResolvedAsync();

        sender.Verify(s => s.Send(It.Is<GetPlanningHolidaysQuery>(q => q.Country == "CH" && q.Region == "TI" && q.Municipality == "Lugano" && q.Year == 2027), Any), Times.Once);
        Assert.True(vm.HasResolved);
        Assert.Equal(new[] { "New Year", "Local fair", "Christmas" }, vm.ResolvedRows.Select(row => row.Name));
        Assert.Equal(new DateOnly(2026, 1, 1).ToString("d", loc.CurrentCulture), vm.ResolvedRows[0].DateText);
        Assert.Equal(0.5m.ToString("0.##", loc.CurrentCulture), vm.ResolvedRows[1].FractionText);
    }

    /// <summary>
    /// A place without holidays says so, and a year that is not a year is explained without asking the server.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ShowResolved_EmptyAndInvalidYear_AreExplained()
    {
        resolved = [];
        var vm = Create();
        vm.ResolveYearText = "abc";

        await vm.ShowResolvedAsync();
        Assert.Equal(loc["ResourcePlanning_Setup_holiday_invalid_date"], vm.StatusMessage);
        sender.Verify(s => s.Send(It.IsAny<GetPlanningHolidaysQuery>(), Any), Times.Never);

        vm.ResolveYearText = "1500";
        await vm.ShowResolvedAsync();
        sender.Verify(s => s.Send(It.IsAny<GetPlanningHolidaysQuery>(), Any), Times.Never);

        vm.ResolveYearText = "2027";
        await vm.ShowResolvedAsync();

        Assert.False(vm.HasResolved);
        Assert.Equal(loc["ResourcePlanning_Holidays_ResolvedEmpty"], vm.StatusMessage);
    }

    /// <summary>
    /// Once the resolved holidays are shown they are refreshed when a local holiday is added, because it may be one of them.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task Add_AfterShowingTheResolvedHolidays_RefreshesThem()
    {
        var vm = Create();
        await vm.ShowResolvedAsync();
        vm.NameText = "Fair";
        vm.DayText = "15";
        vm.MonthText = "8";

        await vm.AddAsync();

        sender.Verify(s => s.Send(It.IsAny<GetPlanningHolidaysQuery>(), Any), Times.Exactly(2));
    }

    /// <summary>
    /// A server that cannot be reached while showing the resolved holidays shows the load failure.
    /// </summary>
    /// <returns>A task representing test execution.</returns>
    [Fact]
    public async Task ShowResolved_ServerDown_ShowsTheLoadFailure()
    {
        _ = sender.Setup(s => s.Send(It.IsAny<GetPlanningHolidaysQuery>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = Create();

        await vm.ShowResolvedAsync();

        Assert.Equal(loc["ResourcePlanning_LoadFailed"], vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    private LocalHolidaysViewModel Create() => new(sender.Object, loc, time);
}
