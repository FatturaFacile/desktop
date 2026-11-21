// <copyright file="ManufacturingMasterData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// Everything one manufacturing sync pulls; it replaces the local cache in a single step.
/// </summary>
/// <param name="WorkCenters">The work centres.</param>
/// <param name="Machines">The machines.</param>
/// <param name="Calendars">The working calendars with shifts and closures.</param>
/// <param name="Routings">The routings with operation steps.</param>
/// <param name="OeeSnapshots">The OEE summaries.</param>
public sealed record ManufacturingMasterData(
    IReadOnlyList<MfgWorkCenter> WorkCenters,
    IReadOnlyList<MfgMachineCenter> Machines,
    IReadOnlyList<MfgWorkingCalendar> Calendars,
    IReadOnlyList<MfgRouting> Routings,
    IReadOnlyList<MfgOeeSnapshot> OeeSnapshots);
