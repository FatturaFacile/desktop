// <copyright file="CreateSwissQrBillCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Command to create and issue a new Swiss QR-bill compliant with SIX interbank standards.
/// </summary>
/// <param name="BillNumber">The sequential reference identifier of the QR-bill.</param>
/// <param name="BillDate">The formal billing issuance date.</param>
/// <param name="ClientId">The unique identifier of the debtor customer.</param>
/// <param name="CreditorName">The full name or corporate denomination of the creditor.</param>
/// <param name="CreditorAddress">The postal address value object of the creditor.</param>
/// <param name="CreditorQrIban">The specialized Swiss QR-IBAN account number of the creditor.</param>
/// <param name="DebtorName">The full name or corporate denomination of the paying debtor.</param>
/// <param name="DebtorAddress">The postal address value object of the debtor.</param>
/// <param name="AmountChf">The payable invoice amount in Swiss Francs.</param>
/// <param name="ReferenceNumber">The structured 27-digit QR-reference number for automated reconciliation.</param>
/// <param name="DueDate">The settlement maturity due date.</param>
/// <param name="Message">Optional unstructured payment reference information.</param>
public record CreateSwissQrBillCommand(
    string BillNumber,
    DateTime BillDate,
    Guid ClientId,
    string CreditorName,
    Address CreditorAddress,
    string CreditorQrIban,
    string DebtorName,
    Address DebtorAddress,
    decimal AmountChf,
    string ReferenceNumber,
    DateTime DueDate,
    string? Message = null) : IRequest<SwissQrBillDto>;
