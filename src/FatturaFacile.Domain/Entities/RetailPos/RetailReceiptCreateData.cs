// <copyright file="RetailReceiptCreateData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.RetailPos;

/// <summary>
/// Data required to create a retail receipt.
/// </summary>
public sealed record RetailReceiptCreateData(
    Guid PosTerminalSessionId,
    string ReceiptNumber,
    decimal TotalAmount,
    Currency Currency,
    PaymentMethod PaymentMethod,
    string? ItalianRtTransmissionLot = null,
    string? ItalianZReportNumber = null,
    string? SwissTerminalTxReference = null);
