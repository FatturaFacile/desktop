// <copyright file="ReceivablesWire.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// Maps the enums of the receivables to and from the strings of the web-api contract.
/// </summary>
public static class ReceivablesWire
{
    /// <summary>
    /// Gets the wire name of a status.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <returns>The wire name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a known member.</exception>
    public static string ToWire(this ReceivableStatus status) => status switch
    {
        ReceivableStatus.Open => "open",
        ReceivableStatus.PartiallyPaid => "partially_paid",
        ReceivableStatus.Paid => "paid",
        ReceivableStatus.Overdue => "overdue",
        ReceivableStatus.Disputed => "disputed",
        ReceivableStatus.WrittenOff => "written_off",
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    /// <summary>
    /// Gets the wire name of an allocation type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The wire name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a known member.</exception>
    public static string ToWire(this AllocationType type) => type switch
    {
        AllocationType.Payment => "payment",
        AllocationType.CreditNote => "credit_note",
        AllocationType.DebitAdjustment => "debit_adjustment",
        AllocationType.WriteOff => "write_off",
        AllocationType.Discount => "discount",
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    /// <summary>
    /// Gets the wire name of a reaction policy.
    /// </summary>
    /// <param name="policy">The policy.</param>
    /// <returns>The wire name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a known member.</exception>
    public static string ToWire(this CreditReactionPolicy policy) => policy switch
    {
        CreditReactionPolicy.Warning => "warning",
        CreditReactionPolicy.BlockOrder => "block_order",
        CreditReactionPolicy.BlockDeliveryNote => "block_delivery_note",
        CreditReactionPolicy.CreditHold => "credit_hold",
        _ => throw new ArgumentOutOfRangeException(nameof(policy)),
    };

    /// <summary>
    /// Gets the wire name of a credit scope.
    /// </summary>
    /// <param name="scope">The scope.</param>
    /// <returns>The wire name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a known member.</exception>
    public static string ToWire(this CreditScope scope) => scope switch
    {
        CreditScope.All => "all",
        CreditScope.Order => "order",
        CreditScope.DeliveryNote => "delivery_note",
        _ => throw new ArgumentOutOfRangeException(nameof(scope)),
    };

    /// <summary>
    /// Gets the wire name of a credit outcome.
    /// </summary>
    /// <param name="outcome">The outcome.</param>
    /// <returns>The wire name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a known member.</exception>
    public static string ToWire(this CreditOutcome outcome) => outcome switch
    {
        CreditOutcome.Ok => "ok",
        CreditOutcome.Warning => "warning",
        CreditOutcome.Block => "block",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
    };

    /// <summary>
    /// Gets the wire name of a risk rating.
    /// </summary>
    /// <param name="rating">The rating.</param>
    /// <returns>The wire name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a known member.</exception>
    public static string ToWire(this RiskRating rating) => rating switch
    {
        RiskRating.Low => "low",
        RiskRating.Medium => "medium",
        RiskRating.High => "high",
        RiskRating.Critical => "critical",
        _ => throw new ArgumentOutOfRangeException(nameof(rating)),
    };

    /// <summary>
    /// Parses a status; an unknown name reads as open.
    /// </summary>
    /// <param name="wire">The wire name.</param>
    /// <returns>The status.</returns>
    public static ReceivableStatus ParseStatus(string? wire) => wire switch
    {
        "partially_paid" => ReceivableStatus.PartiallyPaid,
        "paid" => ReceivableStatus.Paid,
        "overdue" => ReceivableStatus.Overdue,
        "disputed" => ReceivableStatus.Disputed,
        "written_off" => ReceivableStatus.WrittenOff,
        _ => ReceivableStatus.Open,
    };

    /// <summary>
    /// Parses an allocation type; an unknown name reads as a payment.
    /// </summary>
    /// <param name="wire">The wire name.</param>
    /// <returns>The type.</returns>
    public static AllocationType ParseAllocationType(string? wire) => wire switch
    {
        "credit_note" => AllocationType.CreditNote,
        "debit_adjustment" => AllocationType.DebitAdjustment,
        "write_off" => AllocationType.WriteOff,
        "discount" => AllocationType.Discount,
        _ => AllocationType.Payment,
    };

    /// <summary>
    /// Parses a reaction policy; an unknown name reads as a warning.
    /// </summary>
    /// <param name="wire">The wire name.</param>
    /// <returns>The policy.</returns>
    public static CreditReactionPolicy ParsePolicy(string? wire) => wire switch
    {
        "block_order" => CreditReactionPolicy.BlockOrder,
        "block_delivery_note" => CreditReactionPolicy.BlockDeliveryNote,
        "credit_hold" => CreditReactionPolicy.CreditHold,
        _ => CreditReactionPolicy.Warning,
    };

    /// <summary>
    /// Parses a credit scope; an unknown name reads as all.
    /// </summary>
    /// <param name="wire">The wire name.</param>
    /// <returns>The scope.</returns>
    public static CreditScope ParseScope(string? wire) => wire switch
    {
        "order" => CreditScope.Order,
        "delivery_note" => CreditScope.DeliveryNote,
        _ => CreditScope.All,
    };

    /// <summary>
    /// Parses a credit outcome; an unknown name reads as ok.
    /// </summary>
    /// <param name="wire">The wire name.</param>
    /// <returns>The outcome.</returns>
    public static CreditOutcome ParseOutcome(string? wire) => wire switch
    {
        "warning" => CreditOutcome.Warning,
        "block" => CreditOutcome.Block,
        _ => CreditOutcome.Ok,
    };

    /// <summary>
    /// Parses a risk rating; an unknown name reads as low.
    /// </summary>
    /// <param name="wire">The wire name.</param>
    /// <returns>The rating.</returns>
    public static RiskRating ParseRating(string? wire) => wire switch
    {
        "medium" => RiskRating.Medium,
        "high" => RiskRating.High,
        "critical" => RiskRating.Critical,
        _ => RiskRating.Low,
    };
}
