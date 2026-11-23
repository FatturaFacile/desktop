// <copyright file="IntentLetterInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The data of a declaration of intent entered by the user.
/// </summary>
/// <param name="Id">The letter being changed, or null for a new one.</param>
/// <param name="ContactId">The customer (received) or supplier (issued).</param>
/// <param name="Direction">Whether the letter was received or issued.</param>
/// <param name="ProtocolNumber">The telematic protocol of the receipt of the tax authority.</param>
/// <param name="ReceiptDate">The date of the receipt.</param>
/// <param name="Year">The year of the plafond.</param>
/// <param name="PlafondAmount">The authorised plafond.</param>
/// <param name="ValidFrom">The first day of validity.</param>
/// <param name="ValidTo">The last day of validity.</param>
/// <param name="Notes">Free notes.</param>
public sealed record IntentLetterInput(Guid? Id, Guid ContactId, IntentLetterDirection Direction, string ProtocolNumber, DateTime ReceiptDate, int Year, decimal PlafondAmount, DateTime ValidFrom, DateTime ValidTo, string? Notes);
