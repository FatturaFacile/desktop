// <copyright file="SwissVatErrors.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Localization resource keys for Swiss VAT (Mod 024) errors and validation messages.
/// </summary>
public static class SwissVatErrors
{
    /// <summary>
    /// Resource key when the UID-MWST identifier is missing or invalid.
    /// </summary>
    public const string UidMwstInvalid = "SwissVat_Error_UidMwstInvalid";

    /// <summary>
    /// Resource key when Saldosteuersatz is selected without a positive primary rate.
    /// </summary>
    public const string SssRateRequired = "SwissVat_Error_SssRateRequired";

    /// <summary>
    /// Resource key when quadratura validation has blocking failures.
    /// </summary>
    public const string QuadraturaFailed = "SwissVat_Error_QuadraturaFailed";

    /// <summary>
    /// Resource key when SuisseTax eCH-0217 XSD validation fails.
    /// </summary>
    public const string XsdValidationFailed = "SwissVat_Error_XsdValidationFailed";

    /// <summary>
    /// Resource key when Bezugsteuer is applied to a domestic CH/LI supplier.
    /// </summary>
    public const string BezugsteuerDomesticForbidden = "SwissVat_Error_BezugsteuerDomesticForbidden";

    /// <summary>
    /// Resource key when an input tax adjustment has an invalid amount or cost.
    /// </summary>
    public const string AdjustmentAmountInvalid = "SwissVat_Error_AdjustmentAmountInvalid";
}
