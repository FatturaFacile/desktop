// <copyright file="MfgOeeRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An OEE row of the manufacturing view (one cached summary).
/// </summary>
/// <param name="WorkCenterId">The work centre identifier.</param>
/// <param name="WorkCenter">The work centre.</param>
/// <param name="Period">The period text.</param>
/// <param name="Availability">The availability text.</param>
/// <param name="Performance">The performance text.</param>
/// <param name="Quality">The quality text.</param>
/// <param name="Oee">The OEE text.</param>
/// <param name="Target">The target text.</param>
/// <param name="Gap">The gap to the target text.</param>
/// <param name="MeetsTarget">Whether the OEE reaches the target.</param>
internal sealed record MfgOeeRow(Guid WorkCenterId, string WorkCenter, string Period, string Availability, string Performance, string Quality, string Oee, string Target, string Gap, bool MeetsTarget);
