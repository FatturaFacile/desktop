// <copyright file="RebateCodes.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The wire codes of the web-api for supplier rebates and blanket agreements.
/// </summary>
public static class RebateCodes
{
    /// <summary>
    /// The direction of a supplier (purchase) agreement.
    /// </summary>
    public const string DirectionPurchase = "purchase";

    /// <summary>
    /// Tiered volume rebate.
    /// </summary>
    public const string TypeTieredVolume = "tiered_volume";

    /// <summary>
    /// Fixed bonus once the spend target is reached.
    /// </summary>
    public const string TypeFixedBonus = "fixed_bonus";

    /// <summary>
    /// Marketing and listing contributions.
    /// </summary>
    public const string TypeMarketingContribution = "marketing_contribution";

    /// <summary>
    /// The retroactive scale.
    /// </summary>
    public const string ModeRetroactive = "retroactive";

    /// <summary>
    /// The incremental scale.
    /// </summary>
    public const string ModeIncremental = "incremental";

    /// <summary>
    /// A draft agreement.
    /// </summary>
    public const string StatusDraft = "draft";

    /// <summary>
    /// An active agreement.
    /// </summary>
    public const string StatusActive = "active";

    /// <summary>
    /// A closed agreement.
    /// </summary>
    public const string StatusClosed = "closed";

    /// <summary>
    /// A cancelled agreement.
    /// </summary>
    public const string StatusCancelled = "cancelled";

    /// <summary>
    /// Monthly accrual.
    /// </summary>
    public const string FrequencyMonthly = "monthly";

    /// <summary>
    /// Quarterly accrual.
    /// </summary>
    public const string FrequencyQuarterly = "quarterly";

    /// <summary>
    /// Accrual on custom periods.
    /// </summary>
    public const string FrequencyCustom = "custom";

    /// <summary>
    /// A marketing contribution.
    /// </summary>
    public const string KindMarketing = "marketing";

    /// <summary>
    /// A co-marketing contribution.
    /// </summary>
    public const string KindCoMarketing = "co_marketing";

    /// <summary>
    /// A catalogue listing contribution.
    /// </summary>
    public const string KindListing = "listing";

    /// <summary>
    /// The document type of a supplier credit note (counts negative in the qualifying spend).
    /// </summary>
    public const string CreditNoteDocumentType = "TD04";

    /// <summary>
    /// A posted accrual run.
    /// </summary>
    public const string RunPosted = "posted";

    /// <summary>
    /// A reversed accrual run.
    /// </summary>
    public const string RunReversed = "reversed";

    /// <summary>
    /// An alert of a blanket agreement whose ceiling is almost used up.
    /// </summary>
    public const string AlertCeilingNear = "ceiling_near";

    /// <summary>
    /// An alert of a blanket agreement whose ceiling is exceeded.
    /// </summary>
    public const string AlertCeilingExceeded = "ceiling_exceeded";

    /// <summary>
    /// An alert of a blanket agreement whose minimum spend is at risk.
    /// </summary>
    public const string AlertMinimumAtRisk = "minimum_at_risk";

    /// <summary>
    /// An alert of a blanket agreement whose minimum spend was not met.
    /// </summary>
    public const string AlertMinimumNotMet = "minimum_not_met";
}
