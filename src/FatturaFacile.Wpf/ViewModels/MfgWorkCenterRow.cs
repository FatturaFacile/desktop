// <copyright file="MfgWorkCenterRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A work centre row of the manufacturing view.
/// </summary>
/// <param name="Id">The work centre identifier.</param>
/// <param name="Code">The code.</param>
/// <param name="Name">The name.</param>
/// <param name="Status">The localized status.</param>
/// <param name="Capacity">The daily capacity text.</param>
/// <param name="Efficiency">The efficiency text.</param>
/// <param name="Oee">The cached OEE text, or a dash.</param>
/// <param name="Target">The OEE target text.</param>
/// <param name="MachineCount">The number of machines.</param>
internal sealed record MfgWorkCenterRow(Guid Id, string Code, string Name, string Status, string Capacity, string Efficiency, string Oee, string Target, string MachineCount);
