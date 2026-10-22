// <copyright file="QrBillType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Enums;

/// <summary>
/// Specifies the variant of the Swiss QR-bill standard (<c>QR-Rechnung</c>) compliant with SIX Payment Services specifications.
/// </summary>
public enum QrBillType
{
    /// <summary>
    /// QR-bill utilizing a special QR-IBAN coupled with a 27-digit structured numeric QR-reference (successor to the Orange Inpayment Slip / ESR), enabling fully automated debtor matching.
    /// </summary>
    QrIbanWithReference = 1,

    /// <summary>
    /// QR-bill utilizing a conventional IBAN accompanied by an ISO 11649 Structured Creditor Reference (SCOR), standard for international and cross-border SEPA banking.
    /// </summary>
    IbanWithScor = 2,

    /// <summary>
    /// QR-bill utilizing a conventional IBAN without any structured reference number (successor to the Red Inpayment Slip / IS), relying on unstructured textual payment remittance notes.
    /// </summary>
    IbanWithoutReference = 3,
}
