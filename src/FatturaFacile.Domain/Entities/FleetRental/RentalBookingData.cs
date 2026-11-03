// <copyright file="RentalBookingData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Data required to create a rental booking.
/// </summary>
public sealed record RentalBookingData(
    string BookingNumber,
    Guid RentalVehicleId,
    Guid ClientId,
    DateTime StartDate,
    DateTime EndDate,
    string PickupLocation,
    string ReturnLocation,
    Money TotalRentAmount,
    Money DepositAmount);
