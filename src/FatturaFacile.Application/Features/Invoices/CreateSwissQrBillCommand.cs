// <copyright file="CreateSwissQrBillCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.ValueObjects;
using MediatR;

namespace FatturaFacile.Application.Features.Invoices;

/// <summary>
/// Represents create swiss qr bill command.
/// </summary>
/// <param name="BillNumber">The bill number.</param>
/// <param name="BillDate">The bill date.</param>
/// <param name="ClientId">The client id.</param>
/// <param name="CreditorName">The creditor name.</param>
/// <param name="CreditorAddress">The creditor address.</param>
/// <param name="CreditorQrIban">The creditor qr iban.</param>
/// <param name="DebtorName">The debtor name.</param>
/// <param name="DebtorAddress">The debtor address.</param>
/// <param name="AmountChf">The amount chf.</param>
/// <param name="ReferenceNumber">The reference number.</param>
/// <param name="DueDate">The due date.</param>
/// <param name="Message">The message.</param>
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
