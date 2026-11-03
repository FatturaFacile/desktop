// <copyright file="SwissQrBillData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Sales;

/// <summary>
/// Data transfer object holding payee, payer, amounts, and structured reference for creating a Swiss QR-bill.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="BillNumber">The business identifier or invoice number for the bill.</param>
/// <param name="BillDate">The issuance date of the QR-bill.</param>
/// <param name="ClientId">The client or debtor entity identifier.</param>
/// <param name="CreditorName">The legal payee company name.</param>
/// <param name="CreditorAddress">The postal address of the payee.</param>
/// <param name="CreditorQrIban">The validated Swiss QR-IBAN of the creditor.</param>
/// <param name="DebtorName">The legal payer name.</param>
/// <param name="DebtorAddress">The postal address of the payer.</param>
/// <param name="RawAmount">The unrounded monetary sum billed.</param>
/// <param name="ReferenceNumber">The 27-digit structured reference number (Modulo 10 check digit).</param>
/// <param name="DueDate">The contractual maturity date.</param>
/// <param name="Message">An optional unstructured message printed on the payment slip.</param>
public sealed record SwissQrBillData(
    Guid     TenantId,
    string   BillNumber,
    DateTime BillDate,
    Guid     ClientId,
    string   CreditorName,
    Address  CreditorAddress,
    QrIban   CreditorQrIban,
    string   DebtorName,
    Address  DebtorAddress,
    Money    RawAmount,
    string   ReferenceNumber,
    DateTime DueDate,
    string?  Message = null);
