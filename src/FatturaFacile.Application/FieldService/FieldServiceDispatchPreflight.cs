// <copyright file="FieldServiceDispatchPreflight.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Predicts, from the cached board, whether dispatching a work order to a technician in a window will be blocked: the schedule
/// overlap, the missing skills and the van stock the server checks. The server stays the authority; this only lets the
/// dispatcher see the warnings on the timeline before sending.
/// </summary>
public static class FieldServiceDispatchPreflight
{
    private const string Separator = ", ";

    /// <summary>
    /// Evaluates a dispatch.
    /// </summary>
    /// <param name="lane">The technician lane.</param>
    /// <param name="order">The work order to dispatch.</param>
    /// <param name="start">The scheduled start.</param>
    /// <param name="end">The scheduled end.</param>
    /// <returns>The warnings, empty when nothing is expected to block the dispatch.</returns>
    public static IReadOnlyList<FieldServiceWarning> Evaluate(FieldServiceLane lane, FieldServiceBlock order, DateTimeOffset start, DateTimeOffset end)
    {
        ArgumentNullException.ThrowIfNull(lane);
        ArgumentNullException.ThrowIfNull(order);

        List<FieldServiceWarning> warnings = [];

        var overlapping = lane.Blocks
            .Where(block => !string.Equals(block.WorkOrderId, order.WorkOrderId, StringComparison.Ordinal) && Overlaps(block, start, end))
            .Select(block => block.Number)
            .ToList();
        if (overlapping.Count > 0)
        {
            warnings.Add(new FieldServiceWarning(FieldServiceWarningKind.Schedule, string.Join(Separator, overlapping)));
        }

        var missing = MissingSkills(lane, order);
        if (missing.Count > 0)
        {
            warnings.Add(new FieldServiceWarning(FieldServiceWarningKind.Skills, string.Join(Separator, missing)));
        }

        if (!lane.VanReady || (order.RequiredParts > 0 && lane.VanProducts < order.RequiredParts))
        {
            warnings.Add(new FieldServiceWarning(FieldServiceWarningKind.VanStock, string.Empty));
        }

        return warnings;
    }

    /// <summary>
    /// Lists the required skills a technician does not have, comparing without case.
    /// </summary>
    /// <param name="lane">The technician lane.</param>
    /// <param name="order">The work order.</param>
    /// <returns>The missing skills.</returns>
    public static IReadOnlyList<string> MissingSkills(FieldServiceLane lane, FieldServiceBlock order)
    {
        ArgumentNullException.ThrowIfNull(lane);
        ArgumentNullException.ThrowIfNull(order);

        return [.. order.RequiredSkills.Where(skill => !lane.Skills.Contains(skill, StringComparer.OrdinalIgnoreCase))];
    }

    /// <summary>
    /// Turns the `errors` object of a 422 answer of the dispatch endpoint (keys `schedule`, `skills`, `van_stock`) into warnings.
    /// </summary>
    /// <param name="errors">The messages by blocker key.</param>
    /// <returns>The warnings.</returns>
    public static IReadOnlyList<FieldServiceWarning> FromServer(IReadOnlyDictionary<string, IReadOnlyList<string>> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return [.. errors.Select(pair => new FieldServiceWarning(KindOf(pair.Key), string.Join(Separator, pair.Value)))];
    }

    private static bool Overlaps(FieldServiceBlock block, DateTimeOffset start, DateTimeOffset end) =>
        block.Start is { } blockStart && block.End is { } blockEnd && blockStart < end && blockEnd > start;

    private static FieldServiceWarningKind KindOf(string key) => key switch
    {
        "schedule" or "scheduled_end" => FieldServiceWarningKind.Schedule,
        "skills" => FieldServiceWarningKind.Skills,
        "van_stock" => FieldServiceWarningKind.VanStock,
        _ => FieldServiceWarningKind.Other,
    };
}
