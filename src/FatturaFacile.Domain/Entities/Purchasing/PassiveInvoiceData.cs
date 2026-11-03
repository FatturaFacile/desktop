// <copyright file="PassiveInvoiceData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Purchasing;

/// <summary>
/// Data transfer object holding vendor invoice figures, dates, and electronic routing identifiers for creating a <see cref="PassiveInvoice"/>.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="SupplierId">The supplier or vendor identifier.</param>
/// <param name="DocumentNumber">The vendor's invoice number.</param>
/// <param name="DocumentDate">The statutory invoice date.</param>
/// <param name="TotalNet">The total net purchase amount excluding tax.</param>
/// <param name="TotalVat">The total input VAT amount.</param>
/// <param name="Currency">The currency of the invoice.</param>
/// <param name="PaymentDueDate">The payment due date.</param>
/// <param name="SdiId">The Italian SDI transmission identifier, if applicable.</param>
/// <param name="QrPayload">The Swiss QR-code raw text payload, if applicable.</param>
public sealed record PassiveInvoiceData(
    Guid     TenantId,
    Guid     SupplierId,
    string   DocumentNumber,
    DateTime DocumentDate,
    decimal  TotalNet,
    decimal  TotalVat,
    Currency Currency,
    DateTime PaymentDueDate,
    string?  SdiId     = null,
    string?  QrPayload = null);
