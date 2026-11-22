// <copyright file="FieldServiceBilling.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// The billing breakdown of a service report, with the covered and the chargeable part apart.
/// </summary>
/// <param name="Lines">The priced lines.</param>
/// <param name="BillableTotal">The total to invoice.</param>
/// <param name="CoveredTotal">The total covered by contract or warranty.</param>
/// <param name="Currency">The currency.</param>
public sealed record FieldServiceBilling(IReadOnlyList<FieldServiceBillingLine> Lines, decimal BillableTotal, decimal CoveredTotal, string Currency);
