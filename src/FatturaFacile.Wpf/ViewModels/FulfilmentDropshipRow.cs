// <copyright file="FulfilmentDropshipRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A dropshipment of the dropshipment list.
/// </summary>
/// <param name="Id">The dropshipment.</param>
/// <param name="PurchaseOrderId">The supplier order.</param>
/// <param name="Status">The wire status.</param>
/// <param name="StatusText">The localized status.</param>
/// <param name="RecipientText">The recipient with the address.</param>
/// <param name="DdtText">The transport document number.</param>
/// <param name="BillingText">The billing authorisation.</param>
/// <param name="DeliveredText">When it was delivered.</param>
internal sealed record FulfilmentDropshipRow(
    Guid Id,
    Guid PurchaseOrderId,
    string Status,
    string StatusText,
    string RecipientText,
    string DdtText,
    string BillingText,
    string DeliveredText);
