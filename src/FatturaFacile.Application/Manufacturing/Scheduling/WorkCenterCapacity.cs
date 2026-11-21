// <copyright file="WorkCenterCapacity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Manufacturing.Scheduling;

/// <summary>
/// The finite daily capacity of a work centre.
/// </summary>
/// <param name="WorkCenterId">The work centre identifier.</param>
/// <param name="Code">The work centre code.</param>
/// <param name="DailyMinutes">The productive minutes per working day (hours x efficiency x availability).</param>
public sealed record WorkCenterCapacity(Guid WorkCenterId, string Code, decimal DailyMinutes);
