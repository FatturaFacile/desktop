// <copyright file="SupplierRebatesErrors.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SupplierRebates;

/// <summary>
/// The localization keys of the validation errors of the supplier rebate commands. The validators of the application layer carry
/// the key as message; the view model translates it.
/// </summary>
public static class SupplierRebatesErrors
{
    /// <summary>
    /// A required field is empty.
    /// </summary>
    public const string Required = "SupplierRebates_Err_Required";

    /// <summary>
    /// Two tiers of the scale share the same threshold.
    /// </summary>
    public const string TiersDistinct = "SupplierRebates_Err_TiersDistinct";

    /// <summary>
    /// A tiered volume rebate needs at least one tier.
    /// </summary>
    public const string TiersRequired = "SupplierRebates_Err_TiersRequired";

    /// <summary>
    /// A fixed bonus needs its target and its amount.
    /// </summary>
    public const string BonusRequired = "SupplierRebates_Err_BonusRequired";

    /// <summary>
    /// The agreement ends before it starts.
    /// </summary>
    public const string DatesOrder = "SupplierRebates_Err_DatesOrder";

    /// <summary>
    /// An amount, a rate or a threshold is out of range.
    /// </summary>
    public const string Amount = "SupplierRebates_Err_Amount";

    /// <summary>
    /// The currency is not a three letter code.
    /// </summary>
    public const string Currency = "SupplierRebates_Err_Currency";

    /// <summary>
    /// The VAT rate of the proposal is missing or out of range.
    /// </summary>
    public const string TaxRate = "SupplierRebates_Err_TaxRate";

    /// <summary>
    /// A set-off needs the payable to set off against.
    /// </summary>
    public const string InvoiceRequired = "SupplierRebates_Err_InvoiceRequired";

    /// <summary>
    /// The workflow step is not allowed in the status of the proposal.
    /// </summary>
    public const string ActionNotAllowed = "SupplierRebates_Err_ActionNotAllowed";

    /// <summary>
    /// The reason of a reversal is missing or too long.
    /// </summary>
    public const string Reason = "SupplierRebates_Err_Reason";

    /// <summary>
    /// The contribution kind is unknown.
    /// </summary>
    public const string Kind = "SupplierRebates_Err_Kind";
}
