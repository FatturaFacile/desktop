// <copyright file="RentalBookingStatus.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Rental booking lifecycle status.
/// </summary>
public static class RentalBookingStatus
{
    /// <summary>
    /// The booking has been reserved but not yet activated.
    /// </summary>
    public const string Reserved = "Reserved";

    /// <summary>
    /// The booking is currently active.
    /// </summary>
    public const string Active = "Active";

    /// <summary>
    /// The booking has been completed.
    /// </summary>
    public const string Completed = "Completed";

    /// <summary>
    /// The booking has been cancelled.
    /// </summary>
    public const string Cancelled = "Cancelled";
}
