// <copyright file="SwissQrBillDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// Data transfer representation of a Swiss QR-bill compliant with ISO 20022 and SIX payment standards.
/// </summary>
/// <param name="Id">The unique QR-bill document identifier.</param>
/// <param name="TenantId">The tenant organization boundary owning this bill.</param>
/// <param name="BillNumber">The sequential commercial invoice or bill reference number.</param>
/// <param name="BillDate">The formal issuance date of the QR-bill.</param>
/// <param name="ClientId">The unique identifier of the billed customer.</param>
/// <param name="CreditorName">The full name or legal corporate denomination of the payee creditor.</param>
/// <param name="CreditorAddressFormatted">The structured street and postal address of the creditor.</param>
/// <param name="CreditorQrIban">The specialized Swiss QR-IBAN or IBAN account designated to receive the payment.</param>
/// <param name="DebtorName">The full name or legal denomination of the paying debtor.</param>
/// <param name="DebtorAddressFormatted">The structured street and postal address of the debtor.</param>
/// <param name="Amount">The total financial amount due for payment.</param>
/// <param name="Currency">The settlement currency code (strictly "CHF" or "EUR").</param>
/// <param name="ReferenceNumber">The structured 27-digit QR-reference or ISO Creditor Reference used for automated reconciliation.</param>
/// <param name="AdditionalMessage">Optional unstructured payment reference notes or invoice details printed in the payment section.</param>
/// <param name="DueDate">The settlement maturity due date.</param>
/// <param name="IsPaid">A flag indicating whether the QR-bill has been marked as fully settled and cleared.</param>
public record SwissQrBillDto(
    Guid Id,
    Guid TenantId,
    string BillNumber,
    DateTime BillDate,
    Guid ClientId,
    string CreditorName,
    string CreditorAddressFormatted,
    string CreditorQrIban,
    string DebtorName,
    string DebtorAddressFormatted,
    decimal Amount,
    string Currency,
    string ReferenceNumber,
    string? AdditionalMessage,
    DateTime DueDate,
    bool IsPaid);
