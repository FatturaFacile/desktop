// <copyright file="ResourceBookingType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace FatturaFacile.Application.ResourcePlanning;

/// <summary>
/// How firmly an employee is booked.
/// </summary>
[JsonConverter(typeof(ResourceBookingTypeJsonConverter))]
public enum ResourceBookingType
{
    /// <summary>
    /// A tentative booking, for example for an open opportunity; it weighs on the load but is not committed.
    /// </summary>
    SoftTentative = 0,

    /// <summary>
    /// A firm commitment on a project.
    /// </summary>
    HardCommitted = 1,
}
