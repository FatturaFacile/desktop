// <copyright file="ManufacturingConflictResolverTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Manufacturing.Outbox;
using FatturaFacile.Domain.Manufacturing;
using Xunit;

namespace FatturaFacile.UnitTests.Manufacturing;

/// <summary>
/// Tests of the documented reconnect conflict policy (server state wins for the lifecycle, postings are append-only events).
/// </summary>
public sealed class ManufacturingConflictResolverTests
{
    /// <summary>
    /// Verifies the decision matrix of the policy.
    /// </summary>
    /// <param name="kind">The queued mutation.</param>
    /// <param name="server">The server status.</param>
    /// <param name="retries">The re-applications already made.</param>
    /// <param name="expected">The expected decision.</param>
    [Theory]
    [InlineData(ManufacturingMutationKind.Release, ProductionOrderStatus.Released, 0, ConflictDecision.AlreadyApplied)]
    [InlineData(ManufacturingMutationKind.Release, ProductionOrderStatus.InProgress, 0, ConflictDecision.AlreadyApplied)]
    [InlineData(ManufacturingMutationKind.Release, ProductionOrderStatus.Planned, 0, ConflictDecision.Reapply)]
    [InlineData(ManufacturingMutationKind.Release, ProductionOrderStatus.Planned, 1, ConflictDecision.ManualReview)]
    [InlineData(ManufacturingMutationKind.Release, ProductionOrderStatus.Cancelled, 0, ConflictDecision.ServerWins)]
    [InlineData(ManufacturingMutationKind.Start, ProductionOrderStatus.Released, 0, ConflictDecision.Reapply)]
    [InlineData(ManufacturingMutationKind.Start, ProductionOrderStatus.Planned, 0, ConflictDecision.ManualReview)]
    [InlineData(ManufacturingMutationKind.Complete, ProductionOrderStatus.Closed, 0, ConflictDecision.AlreadyApplied)]
    [InlineData(ManufacturingMutationKind.Complete, ProductionOrderStatus.InProgress, 0, ConflictDecision.Reapply)]
    [InlineData(ManufacturingMutationKind.Close, ProductionOrderStatus.InProgress, 0, ConflictDecision.ManualReview)]
    [InlineData(ManufacturingMutationKind.Close, ProductionOrderStatus.Completed, 0, ConflictDecision.Reapply)]
    [InlineData(ManufacturingMutationKind.ConfirmOperation, ProductionOrderStatus.Completed, 0, ConflictDecision.ServerWins)]
    [InlineData(ManufacturingMutationKind.ConfirmOperation, ProductionOrderStatus.Closed, 0, ConflictDecision.ServerWins)]
    [InlineData(ManufacturingMutationKind.ConfirmOperation, ProductionOrderStatus.InProgress, 0, ConflictDecision.Reapply)]
    [InlineData(ManufacturingMutationKind.ConfirmOperation, ProductionOrderStatus.InProgress, 1, ConflictDecision.ManualReview)]
    [InlineData(ManufacturingMutationKind.ConfirmOperation, ProductionOrderStatus.Planned, 0, ConflictDecision.ManualReview)]
    [InlineData(ManufacturingMutationKind.CreateOrder, ProductionOrderStatus.Planned, 0, ConflictDecision.ManualReview)]
    public void Resolve_FollowsThePolicy(ManufacturingMutationKind kind, ProductionOrderStatus server, int retries, ConflictDecision expected)
    {
        var resolution = ManufacturingConflictResolver.Resolve(kind, server, retries);

        Assert.Equal(expected, resolution.Decision);
        Assert.False(string.IsNullOrWhiteSpace(resolution.Reason));
    }

    /// <summary>
    /// When the server order cannot be read, a person decides.
    /// </summary>
    [Fact]
    public void Resolve_UnknownServerState_RequiresManualReview()
    {
        var resolution = ManufacturingConflictResolver.Resolve(ManufacturingMutationKind.Release, serverStatus: null, conflictRetries: 0);

        Assert.Equal(ConflictDecision.ManualReview, resolution.Decision);
    }
}
