// <copyright file="ReceiptAllocation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The gross takings of a day at one VAT rate.
/// </summary>
/// <param name="VatRate">The VAT rate in percent.</param>
/// <param name="Gross">The gross takings at that rate, VAT included.</param>
public sealed record ReceiptAllocation(decimal VatRate, decimal Gross);
