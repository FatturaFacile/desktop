// <copyright file="SwissQrBillRequest.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// The data printed on and encoded in a Swiss QR-bill payment part.
/// </summary>
/// <param name="Account">The creditor IBAN or QR-IBAN (CH or LI).</param>
/// <param name="Creditor">The creditor structured address.</param>
/// <param name="Amount">The amount (null for an open amount).</param>
/// <param name="Currency">CHF or EUR.</param>
/// <param name="Debtor">The debtor structured address (null when unknown).</param>
/// <param name="ReferenceType">"QRR", "SCOR" or "NON".</param>
/// <param name="Reference">The payment reference (empty for "NON").</param>
/// <param name="UnstructuredMessage">Additional unstructured information (max 140 together with bill information).</param>
public sealed record SwissQrBillRequest(
    string Account,
    SwissQrBillAddress Creditor,
    decimal? Amount,
    string Currency,
    SwissQrBillAddress? Debtor,
    string ReferenceType,
    string Reference,
    string? UnstructuredMessage);
