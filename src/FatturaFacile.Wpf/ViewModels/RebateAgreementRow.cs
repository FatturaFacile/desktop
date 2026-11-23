// <copyright file="RebateAgreementRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.SupplierRebates;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// One agreement of the list of the rebate cockpit, formatted for display.
/// </summary>
/// <param name="Id">The agreement identifier.</param>
/// <param name="Code">The agreement code.</param>
/// <param name="Name">The agreement name.</param>
/// <param name="Supplier">The supplier name.</param>
/// <param name="Status">The localized status.</param>
/// <param name="Period">The validity period.</param>
/// <param name="OpenRebate">The open (accrued, not settled) rebate.</param>
/// <param name="Agreement">The agreement behind the row.</param>
internal sealed record RebateAgreementRow(string Id, string Code, string Name, string Supplier, string Status, string Period, string OpenRebate, RebateAgreement Agreement);
