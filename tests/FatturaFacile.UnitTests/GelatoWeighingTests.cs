// <copyright file="GelatoWeighingTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

#pragma warning disable MA0002, MA0003, MA0006 // Test data uses positional literals and plain string comparisons by design.

using System.Globalization;
using System.Net.Http;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Gelato;
using FatturaFacile.Domain.Gelato;
using FatturaFacile.Wpf.ViewModels;
using Moq;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the guided weighing plan and of the weighing station view model.
/// </summary>
public sealed class GelatoWeighingTests
{
    private static readonly CancellationToken Any = It.IsAny<CancellationToken>();

    private static readonly IReadOnlyList<(string IngredientId, string Name, decimal Grams)> Lines =
    [
        ("milk", "Milk", 600m),
        ("sugar", "Sugar", 160m),
        ("cream", "Cream", 100m),
        ("water", "Water", 140m),
    ];

    /// <summary>
    /// The targets are the recipe scaled to the batch and add up exactly to the batch weight.
    /// </summary>
    [Fact]
    public void Create_ScalesLinesToTheBatchWeight()
    {
        var plan = WeighingPlan.Create(Lines, 3500m);

        Assert.Equal(2100m, plan.Steps[0].TargetGrams);
        Assert.Equal(560m, plan.Steps[1].TargetGrams);
        Assert.Equal(3500m, plan.Steps.Sum(static s => s.TargetGrams));
    }

    /// <summary>
    /// The rounding remainder goes to the heaviest line, so thirds still add up to the batch weight.
    /// </summary>
    [Fact]
    public void Create_AssignsTheRoundingRemainderToTheHeaviestLine()
    {
        var plan = WeighingPlan.Create([("a", "A", 1m), ("b", "B", 1m), ("c", "C", 1m)], 100m);

        Assert.Equal(100m, plan.Steps.Sum(static s => s.TargetGrams));
    }

    /// <summary>
    /// Invalid input is rejected.
    /// </summary>
    [Fact]
    public void Create_RejectsAnEmptyRecipeAndANonPositiveBatch()
    {
        Assert.Throws<ArgumentException>(() => WeighingPlan.Create([], 1000m));
        Assert.Throws<ArgumentOutOfRangeException>(() => WeighingPlan.Create(Lines, 0m));
    }

    /// <summary>
    /// A weight inside the tolerance passes; outside it is under or over.
    /// </summary>
    [Fact]
    public void Record_ClassifiesTheWeightAgainstTheTolerance()
    {
        var plan = WeighingPlan.Create(Lines, 1000m, 1m);

        Assert.Equal(WeighingStatus.InTolerance, plan.Record(0, 604m));
        Assert.Equal(WeighingStatus.Under, plan.Record(1, 150m));
        Assert.Equal(WeighingStatus.Over, plan.Record(2, 110m));
        Assert.Equal(3, plan.CurrentIndex);
        Assert.False(plan.IsComplete);
    }

    /// <summary>
    /// The plan completes when every ingredient is weighed and keeps the lot.
    /// </summary>
    [Fact]
    public void Record_CompletesThePlanAndKeepsTheLot()
    {
        var plan = WeighingPlan.Create(Lines.Take(2).ToList(), 760m);

        _ = plan.Record(0, 600m, "LOT-A");
        _ = plan.Record(1, 160m);

        Assert.True(plan.IsComplete);
        Assert.Equal(-1, plan.CurrentIndex);
        Assert.Equal("LOT-A", plan.Steps[0].LotCode);
        Assert.Throws<ArgumentOutOfRangeException>(() => plan.Record(5, 1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => plan.Record(0, -1m));
    }

    /// <summary>
    /// Starting loads the lines and shows the first ingredient; confirming a weight stores the reading and moves on.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task ViewModel_WeighsTheRecipeStepByStep()
    {
        var api = new Mock<IGelatoApiClient>();
        _ = api.Setup(static a => a.GetRecipesAsync("it", Any)).ReturnsAsync([new GelatoRecipeSummary("r1", "R1", "Pistacchio", 1000m, 170m, 260m, 4m)]);
        _ = api.Setup(static a => a.GetRecipeLinesAsync("r1", "it", Any)).ReturnsAsync([new GelatoRecipeLineInfo("sugar", "Sugar", 400m), new GelatoRecipeLineInfo("milk", "Milk", 600m)]);
        var localization = Localization();
        var vm = new GelatoWeighingViewModel(api.Object, localization);

        await vm.LoadAsync();
        vm.SelectedRecipe = vm.Recipes[0];
        vm.BatchWeightGrams = 2000m;
        await vm.StartAsync();

        Assert.Equal(2, vm.Steps.Count);
        Assert.Equal("Sugar", vm.CurrentStep?.Name);
        Assert.Equal(800m, vm.CurrentStep?.TargetGrams);

        vm.MeasuredText = "800";
        await vm.ConfirmWeightAsync();
        api.Verify(a => a.RecordScaleReadingAsync("sugar", 800m, Any), Times.Once);
        Assert.Equal("Milk", vm.CurrentStep?.Name);

        vm.MeasuredText = "1200";
        await vm.ConfirmWeightAsync();
        Assert.True(vm.IsComplete);
        Assert.Null(vm.CurrentStep);
    }

    /// <summary>
    /// A server that cannot be reached shows the offline message and does not throw.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task ViewModel_ReportsAnUnreachableServer()
    {
        var api = new Mock<IGelatoApiClient>();
        _ = api.Setup(static a => a.GetRecipesAsync(It.IsAny<string>(), Any)).ThrowsAsync(new HttpRequestException("down"));
        var vm = new GelatoWeighingViewModel(api.Object, Localization());

        await vm.LoadAsync();

        Assert.Equal("Gelato_Offline", vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    /// <summary>
    /// An invalid typed weight is refused without calling the API.
    /// </summary>
    /// <returns>A task representing the test.</returns>
    [Fact]
    public async Task ViewModel_RefusesAnInvalidWeight()
    {
        var api = new Mock<IGelatoApiClient>();
        _ = api.Setup(static a => a.GetRecipeLinesAsync(It.IsAny<string>(), It.IsAny<string>(), Any)).ReturnsAsync([new GelatoRecipeLineInfo("milk", "Milk", 1000m)]);
        var vm = new GelatoWeighingViewModel(api.Object, Localization())
        {
            SelectedRecipe = new GelatoRecipeSummary("r1", "R1", "Base", 1000m, 0m, 0m, 0m),
        };

        await vm.StartAsync();
        vm.MeasuredText = "abc";
        await vm.ConfirmWeightAsync();

        Assert.Equal("Gelato_InvalidWeight", vm.StatusMessage);
        api.Verify(static a => a.RecordScaleReadingAsync(It.IsAny<string>(), It.IsAny<decimal>(), Any), Times.Never);
    }

    private static ILocalizationService Localization()
    {
        var localization = new Mock<ILocalizationService>();
        _ = localization.SetupGet(static l => l.CurrentCulture).Returns(new CultureInfo("it-CH"));
        _ = localization.Setup(static l => l[It.IsAny<string>()]).Returns((string key) => key);

        return localization.Object;
    }
}
