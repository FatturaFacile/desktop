// <copyright file="CreditRiskCalculator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;

namespace FatturaFacile.Domain.Receivables;

/// <summary>
/// The composite credit risk score: utilisation of the limit (25 points), overdue share (25), the oldest overdue item (25),
/// the average payment lateness (15), the uninsured share (5) and an active block (5). Rating: below 25 low, below 50 medium,
/// below 75 high, else critical.
/// </summary>
public static class CreditRiskCalculator
{
    /// <summary>
    /// The factor key of the utilisation of the limit.
    /// </summary>
    public const string UtilizationKey = "credit_utilization";

    /// <summary>
    /// The factor key of the overdue share.
    /// </summary>
    public const string OverdueShareKey = "overdue_share";

    /// <summary>
    /// The factor key of the oldest overdue item.
    /// </summary>
    public const string MaxDaysKey = "max_days_past_due";

    /// <summary>
    /// The factor key of the payment lateness.
    /// </summary>
    public const string LatenessKey = "payment_lateness_days";

    /// <summary>
    /// The factor key of the uninsured share.
    /// </summary>
    public const string UninsuredKey = "uninsured_share";

    /// <summary>
    /// The factor key of the credit block.
    /// </summary>
    public const string BlockKey = "credit_block";

    private const string PercentFormat = "0.00";

    /// <summary>
    /// Builds the inputs from an exposure.
    /// </summary>
    /// <param name="exposure">The exposure.</param>
    /// <param name="utilizationPercent">The utilisation of the limit, null without a limit.</param>
    /// <param name="paymentLatenessDays">The average payment lateness.</param>
    /// <param name="uninsuredExposure">The exposure the insurance does not cover.</param>
    /// <param name="blocked">Whether the customer is blocked.</param>
    /// <returns>The inputs.</returns>
    public static RiskInputs BuildInputs(CreditExposure exposure, decimal? utilizationPercent, decimal paymentLatenessDays, decimal uninsuredExposure, bool blocked)
    {
        ArgumentNullException.ThrowIfNull(exposure);

        var overdueShare = exposure.OpenInvoices > 0m ? exposure.OverdueAmount / exposure.OpenInvoices : 0m;
        var uninsuredShare = exposure.TotalExposure > 0m ? uninsuredExposure / exposure.TotalExposure : 0m;

        return new RiskInputs(utilizationPercent, overdueShare, exposure.MaxDaysPastDue, paymentLatenessDays, uninsuredShare, blocked);
    }

    /// <summary>
    /// Scores a customer.
    /// </summary>
    /// <param name="inputs">The measurements.</param>
    /// <returns>The score, its rating and its factors.</returns>
    public static CreditRiskScore Assess(RiskInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var utilizationPoints = UtilizationPoints(inputs.UtilizationPercent);
        var overdueShare = Math.Clamp(inputs.OverdueShare, 0m, 1m);
        var uninsuredShare = Math.Clamp(inputs.UninsuredShare, 0m, 1m);
        var maxDays = Math.Max(0, inputs.MaxDaysPastDue);
        var lateness = Math.Max(0m, inputs.PaymentLatenessDays);

        RiskFactor[] factors =
        [
            new(UtilizationKey, inputs.UtilizationPercent?.ToString(PercentFormat, CultureInfo.InvariantCulture), Points(utilizationPoints)),
            new(OverdueShareKey, (overdueShare * 100m).ToString(PercentFormat, CultureInfo.InvariantCulture), Points(overdueShare * 25m)),
            new(MaxDaysKey, maxDays.ToString(CultureInfo.InvariantCulture), Points(Math.Min(120, maxDays) / 120m * 25m)),
            new(LatenessKey, lateness.ToString(PercentFormat, CultureInfo.InvariantCulture), Points(Math.Min(60m, lateness) / 60m * 15m)),
            new(UninsuredKey, (uninsuredShare * 100m).ToString(PercentFormat, CultureInfo.InvariantCulture), Points(uninsuredShare * 5m)),
            new(BlockKey, inputs.Blocked ? "1" : "0", inputs.Blocked ? 5m : 0m),
        ];

        var score = (int)Math.Round(Math.Min(100m, factors.Sum(factor => factor.Points)), 0, MidpointRounding.AwayFromZero);

        return new CreditRiskScore(score, Rate(score), factors, inputs.Blocked);
    }

    /// <summary>
    /// Gets the rating of a score.
    /// </summary>
    /// <param name="score">The score.</param>
    /// <returns>The rating.</returns>
    public static RiskRating Rate(int score) => score switch
    {
        < 25 => RiskRating.Low,
        < 50 => RiskRating.Medium,
        < 75 => RiskRating.High,
        _ => RiskRating.Critical,
    };

    private static decimal Points(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal UtilizationPoints(decimal? utilizationPercent)
    {
        if (utilizationPercent is not { } utilization)
        {
            return 0m;
        }

        var over = utilization > 100m ? 5m : 0m;

        return Math.Min(25m, Math.Max(0m, (utilization - 70m) / 30m * 20m) + over);
    }
}
