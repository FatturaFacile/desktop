// <copyright file="MfgMachineRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A machine row of the manufacturing view.
/// </summary>
/// <param name="Code">The code.</param>
/// <param name="Name">The name.</param>
/// <param name="Status">The localized status.</param>
/// <param name="CycleTime">The ideal cycle time text.</param>
/// <param name="Telemetry">The localized telemetry status.</param>
/// <param name="Flags">The Industria 4.0 and Transizione 5.0 flags.</param>
internal sealed record MfgMachineRow(string Code, string Name, string Status, string CycleTime, string Telemetry, string Flags);
