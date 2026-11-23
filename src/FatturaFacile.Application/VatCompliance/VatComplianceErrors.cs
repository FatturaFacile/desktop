// <copyright file="VatComplianceErrors.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The localization keys of the validation errors and of the blocks of the Italian VAT compliance module (Mod 029). The user
/// interface resolves them through the resource files.
/// </summary>
public static class VatComplianceErrors
{
    /// <summary>
    /// A required value is missing or too long.
    /// </summary>
    public const string Required = "VatCompliance_Err_Required";

    /// <summary>
    /// An amount is out of range.
    /// </summary>
    public const string Amount = "VatCompliance_Err_Amount";

    /// <summary>
    /// The end of a period is before its start.
    /// </summary>
    public const string DatesOrder = "VatCompliance_Err_DatesOrder";

    /// <summary>
    /// The pro-rata percentage is not between 0 and 100.
    /// </summary>
    public const string Percent = "VatCompliance_Err_Percent";

    /// <summary>
    /// The protocol of an intent letter does not look like a telematic protocol of the Agenzia delle Entrate.
    /// </summary>
    public const string ProtocolFormat = "VatCompliance_Err_ProtocolFormat";

    /// <summary>
    /// An intent letter with the same protocol exists.
    /// </summary>
    public const string ProtocolDuplicate = "VatCompliance_Err_ProtocolDuplicate";

    /// <summary>
    /// The letter has already been used by invoices and cannot be removed or lowered below its erosion.
    /// </summary>
    public const string LetterInUse = "VatCompliance_Err_LetterInUse";

    /// <summary>
    /// The register range contains lines that were not printed before it.
    /// </summary>
    public const string PrintSequence = "VatCompliance_Err_PrintSequence";

    /// <summary>
    /// The register range has no lines to print.
    /// </summary>
    public const string NothingToPrint = "VatCompliance_Err_NothingToPrint";

    /// <summary>
    /// The protocol numbers of the register range are not continuous.
    /// </summary>
    public const string ProtocolGap = "VatCompliance_Err_ProtocolGap";

    /// <summary>
    /// The VAT return was not found.
    /// </summary>
    public const string ReturnNotFound = "VatCompliance_Err_ReturnNotFound";

    /// <summary>
    /// The LIPE communication was not found.
    /// </summary>
    public const string SubmissionNotFound = "VatCompliance_Err_SubmissionNotFound";

    /// <summary>
    /// The LIPE communication does not pass the validation.
    /// </summary>
    public const string SubmissionInvalid = "VatCompliance_Err_SubmissionInvalid";

    /// <summary>
    /// The file attached is not signed.
    /// </summary>
    public const string NotSigned = "VatCompliance_Err_NotSigned";

    /// <summary>
    /// The signed file does not carry the same content as the generated XML.
    /// </summary>
    public const string SignatureMismatch = "VatCompliance_Err_SignatureMismatch";

    /// <summary>
    /// The LIPE communication is not in a state that allows the change.
    /// </summary>
    public const string SubmissionState = "VatCompliance_Err_SubmissionState";

    /// <summary>
    /// The F24 payment was not found.
    /// </summary>
    public const string PaymentNotFound = "VatCompliance_Err_PaymentNotFound";

    /// <summary>
    /// The register line was already printed definitively and is locked.
    /// </summary>
    public const string RegisterLocked = "VatCompliance_Err_RegisterLocked";

    /// <summary>
    /// The invoice cannot be issued: no valid intent letter of the customer covers its date.
    /// </summary>
    public const string BlockNoValidLetter = "VatCompliance_Block_no_valid_letter";

    /// <summary>
    /// The invoice cannot be issued: it would exceed the plafond of the intent letter.
    /// </summary>
    public const string BlockPlafondExceeded = "VatCompliance_Block_plafond_exceeded";
}
