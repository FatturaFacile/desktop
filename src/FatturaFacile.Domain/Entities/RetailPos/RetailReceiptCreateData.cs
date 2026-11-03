// <copyright file="RetailReceiptCreateData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.RetailPos;

/// <summary>
/// Data required to create a retail receipt.
/// </summary>
/// <param name="PosTerminalSessionId">The unique identifier of the POS terminal session.</param>
/// <param name="ReceiptNumber">The receipt number.</param>
/// <param name="TotalAmount">The total amount of the receipt.</param>
/// <param name="Currency">The currency of the total amount.</param>
/// <param name="PaymentMethod">The payment method used.</param>
/// <param name="ItalianRtTransmissionLot">The Italian RT transmission lot, if applicable.</param>
/// <param name="ItalianZReportNumber">The Italian Z report number, if applicable.</param>
/// <param name="SwissTerminalTxReference">The Swiss terminal transaction reference, if applicable.</param>
public sealed record RetailReceiptCreateData(
    Guid PosTerminalSessionId,
    string ReceiptNumber,
    decimal TotalAmount,
    Currency Currency,
    PaymentMethod PaymentMethod,
    string? ItalianRtTransmissionLot = null,
    string? ItalianZReportNumber = null,
    string? SwissTerminalTxReference = null);
