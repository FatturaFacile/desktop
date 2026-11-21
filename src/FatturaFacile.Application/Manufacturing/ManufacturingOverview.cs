// <copyright file="ManufacturingOverview.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Manufacturing;

/// <summary>
/// The cached manufacturing data of the active tenant.
/// </summary>
/// <param name="WorkCenters">The work centres by code.</param>
/// <param name="Machines">The machines by code.</param>
/// <param name="Calendars">The working calendars by code.</param>
/// <param name="Routings">The routings with their operation steps by number and revision.</param>
/// <param name="OeeSnapshots">The OEE summaries.</param>
/// <param name="LastSyncedUtc">When the data was last pulled, or null when never.</param>
public sealed record ManufacturingOverview(
    IReadOnlyList<MfgWorkCenter> WorkCenters,
    IReadOnlyList<MfgMachineCenter> Machines,
    IReadOnlyList<MfgWorkingCalendar> Calendars,
    IReadOnlyList<MfgRouting> Routings,
    IReadOnlyList<MfgOeeSnapshot> OeeSnapshots,
    DateTime? LastSyncedUtc);
