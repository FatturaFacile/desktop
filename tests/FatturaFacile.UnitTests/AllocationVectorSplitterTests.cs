// <copyright file="AllocationVectorSplitterTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.AnalyticalAccounting;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the analytical allocation vectors: validation, exact Largest Remainder splitting and the API payload of a draft entry.
/// </summary>
public sealed class AllocationVectorSplitterTests
{
    /// <summary>
    /// Percentages are split with the largest remainder, so the parts add up to the line amount.
    /// </summary>
    [Fact]
    public void Split_ByPercentage_AddsUpExactly()
    {
        var result = AllocationVectorSplitter.Split(100.01m, [new(CostCenterId: "a", Percentage: 50m), new(CostCenterId: "b", Percentage: 25m), new(ProfitCenterId: "c", Percentage: 25m)]);

        Assert.True(result.IsValid);
        Assert.Equal([50.01m, 25m, 25m], result.Amounts);
        Assert.Equal(100.01m, result.Amounts.Sum());
    }

    /// <summary>
    /// Three equal shares of one euro leave a cent that goes to the first share.
    /// </summary>
    [Fact]
    public void Split_ThreeWays_GivesTheLeftoverCentToTheFirst()
    {
        var result = AllocationVectorSplitter.Split(1m, [new(CostCenterId: "a", Percentage: 33.3333m), new(CostCenterId: "b", Percentage: 33.3333m), new(CostCenterId: "c", Percentage: 33.3334m)]);

        Assert.True(result.IsValid);
        Assert.Equal(1m, result.Amounts.Sum());
    }

    /// <summary>
    /// Exact amounts must add up to the line.
    /// </summary>
    [Fact]
    public void Split_ByAmount_RequiresTheLineTotal()
    {
        Assert.True(AllocationVectorSplitter.Split(100m, [new(CostCenterId: "a", Amount: 60m), new(ProjectId: "p", Amount: 40m)]).IsValid);

        var wrong = AllocationVectorSplitter.Split(100m, [new(CostCenterId: "a", Amount: 60m), new(ProjectId: "p", Amount: 30m)]);

        Assert.False(wrong.IsValid);
        Assert.Contains("Analytical_VectorAmountSum", wrong.Errors, StringComparer.Ordinal);
        Assert.Empty(wrong.Amounts);
    }

    /// <summary>
    /// Every broken rule is reported with its resource key.
    /// </summary>
    [Fact]
    public void Split_InvalidVectors_ReportTheRule()
    {
        Assert.Contains("Analytical_VectorRequired", AllocationVectorSplitter.Split(10m, []).Errors, StringComparer.Ordinal);
        Assert.Contains("Analytical_VectorPercentSum", AllocationVectorSplitter.Split(10m, [new(CostCenterId: "a", Percentage: 60m), new(CostCenterId: "b", Percentage: 30m)]).Errors, StringComparer.Ordinal);
        Assert.Contains("Analytical_VectorMixedModes", AllocationVectorSplitter.Split(10m, [new(CostCenterId: "a", Amount: 6m), new(CostCenterId: "b", Percentage: 40m)]).Errors, StringComparer.Ordinal);
        Assert.Contains("Analytical_VectorTargetRequired", AllocationVectorSplitter.Split(10m, [new(Percentage: 100m)]).Errors, StringComparer.Ordinal);
        Assert.Contains("Analytical_VectorValueRequired", AllocationVectorSplitter.Split(10m, [new(CostCenterId: "a")]).Errors, StringComparer.Ordinal);
    }

    /// <summary>
    /// A negative amount (a credit) splits into negative parts that still add up.
    /// </summary>
    [Fact]
    public void Allocate_NegativeTotal_KeepsTheSumAndTheOrder()
    {
        Assert.Equal([-34L, -33L, -33L], AllocationVectorSplitter.Allocate(-100, [1m, 1m, 1m]));
        Assert.Equal([0L, 0L], AllocationVectorSplitter.Allocate(100, [0m, 0m]));
    }

    /// <summary>
    /// The shares always add up to the total, whatever the weights.
    /// </summary>
    /// <param name="total">The cents to split.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(99)]
    [InlineData(12345)]
    [InlineData(999999)]
    public void Allocate_AlwaysAddsUpToTheTotal(long total)
    {
        var shares = AllocationVectorSplitter.Allocate(total, [17.3333m, 0.0001m, 99.9m, 3.14159m, 42m]);

        Assert.Equal(total, shares.Sum());
    }

    /// <summary>
    /// A draft entry becomes the API payload with its vectors, and an incomplete one is refused before it is queued.
    /// </summary>
    [Fact]
    public void Draft_ToApiJson_CarriesTheVectorsAndValidates()
    {
        var draft = new AnalyticalEntryDraft(new DateOnly(2027, 3, 15), "Rent", [new AnalyticalEntryLineDraft(100m, "FIXED_DIRECT", Vectors: [new(CostCenterId: "a", Percentage: 60m), new(ProfitCenterId: "b", Percentage: 40m)])]);

        using var document = JsonDocument.Parse(draft.ToApiJson());
        var line = document.RootElement.GetProperty("lines")[0];

        Assert.Empty(draft.Validate());
        Assert.Equal("2027-03-15", document.RootElement.GetProperty("posting_date").GetString());
        Assert.Equal("100.00", line.GetProperty("amount").GetString());
        Assert.Equal(2, line.GetProperty("vectors").GetArrayLength());
        Assert.Contains("Analytical_EntryNeedsLines", new AnalyticalEntryDraft(new DateOnly(2027, 3, 15), Description: null, []).Validate(), StringComparer.Ordinal);
        Assert.Contains("Analytical_InvalidCostElement", new AnalyticalEntryDraft(new DateOnly(2027, 3, 15), Description: null, [new AnalyticalEntryLineDraft(1m, "BOGUS", CostCenterId: "a")]).Validate(), StringComparer.Ordinal);
        Assert.Contains("Analytical_VectorTargetRequired", new AnalyticalEntryDraft(new DateOnly(2027, 3, 15), Description: null, [new AnalyticalEntryLineDraft(1m, "REVENUE")]).Validate(), StringComparer.Ordinal);
    }

    /// <summary>
    /// The enum values are the ones of the web-api contract.
    /// </summary>
    [Fact]
    public void Contract_MatchesTheWebApiEnums()
    {
        Assert.Equal(["STRUCTURAL", "AUXILIARY", "PRODUCTION", "LOGISTICS", "COMMERCIAL", "PROFIT"], AnalyticalContract.CenterCategories);
        Assert.Equal(["AREA_SQM", "HEADCOUNT_FTE", "MACHINE_HOURS", "REVENUE_VOLUME", "FIXED_PERCENTAGE", "CUSTOM"], AnalyticalContract.DriverTypes);
        Assert.Equal(["VARIABLE_DIRECT", "FIXED_DIRECT", "INDIRECT_AUXILIARY", "REVENUE", "NON_OPERATING"], AnalyticalContract.CostElementTypes);
        Assert.Equal(["PREVIEW", "CONSOLIDATED", "REVERSED"], AnalyticalContract.RunStatuses);
        Assert.Equal(["COST_CENTER", "PROFIT_CENTER"], AnalyticalContract.CenterTypes);
    }
}
