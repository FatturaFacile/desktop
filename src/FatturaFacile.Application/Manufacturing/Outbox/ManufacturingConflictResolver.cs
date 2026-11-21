// <copyright file="ManufacturingConflictResolver.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Manufacturing;

namespace FatturaFacile.Application.Manufacturing.Outbox;

/// <summary>
/// The conflict policy applied on reconnect, when the server answers a queued mutation with 409/412.
/// The full policy is documented in docs/manufacturing-offline-sync.md.
/// </summary>
/// <remarks>
/// Server state wins for the lifecycle; the shop-floor postings are append-only events and are never merged by content.
/// A transition whose target the server already reached is treated as applied. A transition the server state now allows is sent once more.
/// Anything ambiguous blocks the order (not the whole queue) until a person decides.
/// </remarks>
public static class ManufacturingConflictResolver
{
    /// <summary>
    /// The number of automatic re-applications before a person has to decide.
    /// </summary>
    public const int MaxAutomaticRetries = 1;

    /// <summary>
    /// Decides what to do with a conflicting entry.
    /// </summary>
    /// <param name="kind">The mutation kind.</param>
    /// <param name="serverStatus">The current server status of the order, or null when it could not be read.</param>
    /// <param name="conflictRetries">The re-applications already attempted.</param>
    /// <returns>The decision with its reason.</returns>
    public static ConflictResolution Resolve(ManufacturingMutationKind kind, ProductionOrderStatus? serverStatus, int conflictRetries)
    {
        if (kind == ManufacturingMutationKind.CreateOrder)
        {
            return Manual("The server rejected the order creation (possible duplicate).");
        }

        if (serverStatus is not { } status)
        {
            return Manual("The server order could not be read.");
        }

        if (status == ProductionOrderStatus.Cancelled)
        {
            return new ConflictResolution(ConflictDecision.ServerWins, "The order was cancelled on the server.");
        }

        var canRetry = conflictRetries < MaxAutomaticRetries;
        return kind switch
        {
            ManufacturingMutationKind.Release => Transition(status, ProductionOrderStatus.Released, ProductionOrderStatus.Planned, canRetry),
            ManufacturingMutationKind.Start => Transition(status, ProductionOrderStatus.InProgress, ProductionOrderStatus.Released, canRetry),
            ManufacturingMutationKind.Complete => Transition(status, ProductionOrderStatus.Completed, ProductionOrderStatus.InProgress, canRetry),
            ManufacturingMutationKind.Close => Transition(status, ProductionOrderStatus.Closed, ProductionOrderStatus.Completed, canRetry),
            ManufacturingMutationKind.ConfirmOperation => Confirmation(status, canRetry),
            ManufacturingMutationKind.CreateOrder => Manual("The server rejected the order creation (possible duplicate)."),
            _ => Manual("Unknown mutation kind."),
        };
    }

    private static ConflictResolution Manual(string reason) => new(ConflictDecision.ManualReview, reason);

    private static ConflictResolution Transition(ProductionOrderStatus server, ProductionOrderStatus target, ProductionOrderStatus required, bool canRetry)
    {
        return (server >= target, server == required && canRetry) switch
        {
            (true, _) => new ConflictResolution(ConflictDecision.AlreadyApplied, $"The server order is already {server}."),
            (_, true) => new ConflictResolution(ConflictDecision.Reapply, $"The server order is {server}: the transition is applied again."),
            _ => Manual($"The server order is {server}, the transition to {target} cannot be applied."),
        };
    }

    private static ConflictResolution Confirmation(ProductionOrderStatus server, bool canRetry)
    {
        if (server is ProductionOrderStatus.Completed or ProductionOrderStatus.Closed)
        {
            return new ConflictResolution(ConflictDecision.ServerWins, $"The order is already {server} on the server: the posting is no longer accepted.");
        }

        var executable = server is ProductionOrderStatus.Released or ProductionOrderStatus.InProgress;
        return executable && canRetry
            ? new ConflictResolution(ConflictDecision.Reapply, "The posting is applied again against the current server state.")
            : Manual($"The server order is {server}: the posting cannot be applied.");
    }
}
