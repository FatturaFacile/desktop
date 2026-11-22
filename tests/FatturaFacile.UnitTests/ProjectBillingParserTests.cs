// <copyright file="ProjectBillingParserTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json;
using FatturaFacile.Application.ProjectBilling;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the project billing read models: parsing, the live certificates and the SAL arithmetic of the desktop.
/// </summary>
public sealed class ProjectBillingParserTests
{
    private static readonly DateTime At = new(2027, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// The summary carries the position, the retention held and the WIP nature.
    /// </summary>
    [Fact]
    public void ParseSnapshot_ReadsTheBillingPosition()
    {
        var snapshot = ProjectBillingParser.ParseSnapshot(ProjectBillingTestData.ProjectId, ProjectBillingTestData.Payload(), At);

        Assert.Equal("CHF", snapshot.Summary.Currency);
        Assert.Equal(240m, snapshot.Summary.UnbilledTimeAndMaterials);
        Assert.Equal(1, snapshot.Summary.UnbilledMilestones);
        Assert.Equal(1000m, snapshot.Summary.RetentionHeld);
        Assert.NotNull(snapshot.Summary.LatestWip);
        Assert.True(snapshot.Summary.LatestWip.IsContractLiability);
        Assert.False(snapshot.Summary.LatestWip.IsContractAsset);
        Assert.Equal(new DateOnly(2027, 3, 31), snapshot.Summary.LatestWip.PeriodEnd);
        Assert.Equal(At, snapshot.SyncedAtUtc);
    }

    /// <summary>
    /// The certificates keep their figures and the last live one is the base of the next.
    /// </summary>
    [Fact]
    public void ParseSnapshot_ReadsTheCertificatesAndKeepsTheLastLiveOne()
    {
        var snapshot = ProjectBillingParser.ParseSnapshot(ProjectBillingTestData.ProjectId, ProjectBillingTestData.Payload(), At);

        Assert.Equal(2, snapshot.Certificates.Count);
        Assert.False(snapshot.Certificates[1].IsLive);
        Assert.False(snapshot.Certificates[0].IsOpen);
        Assert.Equal(new DateOnly(2027, 1, 31), snapshot.Certificates[0].PeriodEnd);
        Assert.Equal(1, snapshot.LastLive?.Number);
        Assert.Equal(25500m, snapshot.Certificates[0].Net);
    }

    /// <summary>
    /// The retainer burn-down and the retention are read.
    /// </summary>
    [Fact]
    public void ParseSnapshot_ReadsTheRetainerAndTheRetention()
    {
        var snapshot = ProjectBillingParser.ParseSnapshot(ProjectBillingTestData.ProjectId, ProjectBillingTestData.Payload(), At);

        Assert.NotNull(snapshot.Retainer);
        Assert.True(snapshot.Retainer.IsLow);
        Assert.True(snapshot.Retainer.ByHours);
        Assert.Equal(90m, snapshot.Retainer.BurnedPercent);
        Assert.Equal(4m, snapshot.Retainer.RemainingHours);
        Assert.NotNull(snapshot.Retention);
        Assert.Equal(500m, snapshot.Retention.Released);
        Assert.Equal("on_acceptance", snapshot.Retention.ReleaseRule);
    }

    /// <summary>
    /// A project without a contract has no terms, retainer or retention, and no SAL calculator.
    /// </summary>
    [Fact]
    public void ParseSnapshot_WithoutContract_HasNoTermsAndNoCalculator()
    {
        var snapshot = ProjectBillingParser.ParseSnapshot(ProjectBillingTestData.ProjectId, ProjectBillingTestData.PayloadWithoutContract(), At);

        Assert.Null(snapshot.Contract);
        Assert.Null(snapshot.Retainer);
        Assert.Null(snapshot.Retention);
        Assert.Empty(snapshot.Certificates);
        Assert.Null(snapshot.PreviewNext(50m));
    }

    /// <summary>
    /// A summary that is not an object is refused, so it never replaces a good cache.
    /// </summary>
    [Fact]
    public void ParseSnapshot_SummaryThatIsNotAnObject_Throws()
    {
        var payload = ProjectBillingTestData.Payload() with { SummaryJson = "[]" };

        _ = Assert.Throws<JsonException>(() => ProjectBillingParser.ParseSnapshot(ProjectBillingTestData.ProjectId, payload, At));
    }

    /// <summary>
    /// The next SAL is previewed against the live certificates and the terms of the contract.
    /// </summary>
    [Fact]
    public void PreviewNext_UsesTheLiveCertificatesAndTheContractTerms()
    {
        var snapshot = ProjectBillingParser.ParseSnapshot(ProjectBillingTestData.ProjectId, ProjectBillingTestData.Payload(), At);

        var preview = snapshot.PreviewNext(55m);

        Assert.NotNull(preview);
        Assert.Equal(25000m, preview.CurrentGross);
        Assert.Equal(2500m, preview.AdvanceDeduction);
        Assert.Equal(1250m, preview.RetentionWithheld);
        Assert.Equal(21250m, preview.NetBillable);
        Assert.Null(snapshot.PreviewNext(30m));
    }

    /// <summary>
    /// An Italian public contract (CIG) without a stated retention withholds 0.50 percent.
    /// </summary>
    [Fact]
    public void EffectiveRetention_ItalianPublicContract_IsHalfAPercent()
    {
        var contract = ProjectBillingTestData.Contract.Replace("\"retention_guarantee_percent\": \"5.0000\"", "\"retention_guarantee_percent\": \"0.0000\"", StringComparison.Ordinal).Replace("\"cig\": null", "\"cig\": \"Z1234567AB\"", StringComparison.Ordinal);
        var snapshot = ProjectBillingParser.ParseSnapshot(ProjectBillingTestData.ProjectId, ProjectBillingTestData.Payload() with { ContractJson = contract, CertificatesJson = "[]" }, At);

        Assert.Equal(0.5m, snapshot.Contract?.EffectiveRetentionPercent);
        Assert.Equal(200m, snapshot.PreviewNext(40m)?.RetentionWithheld);
    }

    /// <summary>
    /// The first SAL recoups the advance in proportion and withholds the retention.
    /// </summary>
    [Fact]
    public void Calculator_FirstSal_RecoupsInProportionAndWithholds()
    {
        var sal = ProjectBillingSalCalculator.Preview(100_000m, 30m, 0m, 0m, 10000m, 0m, 5m);

        Assert.NotNull(sal);
        Assert.Equal(30000m, sal.CurrentGross);
        Assert.Equal(3000m, sal.AdvanceDeduction);
        Assert.Equal(1500m, sal.RetentionWithheld);
        Assert.Equal(27000m, sal.InvoiceAmount);
        Assert.Equal(25500m, sal.NetBillable);
        Assert.False(sal.IsFinal);
    }

    /// <summary>
    /// The final SAL recoups the whole remaining advance.
    /// </summary>
    [Fact]
    public void Calculator_FinalSal_RecoupsTheRemainingAdvance()
    {
        var sal = ProjectBillingSalCalculator.Preview(100_000m, 100m, 90m, 90_000m, 10000m, 8000m, 0m);

        Assert.NotNull(sal);
        Assert.True(sal.IsFinal);
        Assert.Equal(10000m, sal.CurrentGross);
        Assert.Equal(2000m, sal.AdvanceDeduction);
    }

    /// <summary>
    /// The advance never exceeds the gross of the certificate.
    /// </summary>
    [Fact]
    public void Calculator_HugeAdvance_IsCappedByTheGross()
    {
        var sal = ProjectBillingSalCalculator.Preview(100_000m, 10m, 0m, 0m, 1_000_000m, 0m, 0m);

        Assert.NotNull(sal);
        Assert.Equal(10000m, sal.AdvanceDeduction);
        Assert.Equal(0m, sal.InvoiceAmount);
    }

    /// <summary>
    /// A progress that does not advance, exceeds 100 percent or has no contract value is refused.
    /// </summary>
    /// <param name="value">The contract value.</param>
    /// <param name="cumulative">The cumulative completion.</param>
    /// <param name="previous">The previous completion.</param>
    [Theory]
    [InlineData(100000, 30, 30)]
    [InlineData(100000, 101, 0)]
    [InlineData(0, 10, 0)]
    public void Calculator_InvalidProgress_ReturnsNull(int value, int cumulative, int previous) =>
        Assert.Null(ProjectBillingSalCalculator.Preview(value, cumulative, previous, 0m, 0m, 0m, 0m));
}
