// <copyright file="RentalBookingData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Data required to create a rental booking.
/// </summary>
/// <param name="BookingNumber">The booking number.</param>
/// <param name="RentalVehicleId">The unique identifier of the rental vehicle.</param>
/// <param name="ClientId">The unique identifier of the client.</param>
/// <param name="StartDate">The start date of the rental period.</param>
/// <param name="EndDate">The end date of the rental period.</param>
/// <param name="PickupLocation">The location where the vehicle will be picked up.</param>
/// <param name="ReturnLocation">The location where the vehicle will be returned.</param>
/// <param name="TotalRentAmount">The total rent amount.</param>
/// <param name="DepositAmount">The deposit amount.</param>
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
