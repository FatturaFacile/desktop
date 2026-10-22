// <copyright file="RentalBooking.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Represents a vehicle rental contract booking or reservation within a fleet leasing operation,
/// tracking hire schedules, handover branches, rental tariffs, pre-authorized security deposits,
/// and agreement lifecycles.
/// </summary>
public sealed class RentalBooking : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private RentalBooking()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant rental company.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique rental agreement or reservation reference number (e.g., "BK-2026/001").
    /// </summary>
    public string BookingNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier of the rental fleet vehicle assigned to this booking.
    /// </summary>
    public Guid RentalVehicleId { get; private set; }

    /// <summary>
    /// Gets the unique identifier of the customer or corporate client renting the vehicle.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the start date and time of the vehicle rental period.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the agreed return date and time when the vehicle is due back at the depot.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Gets the designated station, branch, or airport location for vehicle pick-up.
    /// </summary>
    public string PickupLocation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the agreed return depot or drop-off location.
    /// </summary>
    public string ReturnLocation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the total agreed monetary rental price including base rates and selected add-ons.
    /// </summary>
    public Money TotalRentAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the security guarantee deposit amount held or pre-authorized against damages or fuel discrepancies.
    /// </summary>
    public Money DepositAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the operational lifecycle status of the rental contract (e.g., "Reserved", "Active", "Completed", "Cancelled").
    /// </summary>
    public string Status { get; private set; } = "Reserved";

    /// <summary>
    /// Gets or sets the UTC timestamp when this rental booking was initiated.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who created this reservation.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this rental booking was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier or username of the user who last modified this reservation.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new vehicle rental booking reservation.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="bookingNumber">The unique reservation or voucher reference code.</param>
    /// <param name="vehicleId">The fleet vehicle identifier assigned to the reservation.</param>
    /// <param name="clientId">The renting customer identifier.</param>
    /// <param name="start">The scheduled vehicle collection timestamp.</param>
    /// <param name="end">The scheduled vehicle return timestamp.</param>
    /// <param name="pickup">The vehicle handover depot location name.</param>
    /// <param name="ret">The vehicle return depot location name.</param>
    /// <param name="total">The total agreed monetary rental fee.</param>
    /// <param name="deposit">The pre-authorized security deposit amount.</param>
    /// <returns>A newly created <see cref="RentalBooking"/> aggregate root in "Reserved" status.</returns>
    public static RentalBooking Create(
        Guid tenantId,
        string bookingNumber,
        Guid vehicleId,
        Guid clientId,
        DateTime start,
        DateTime end,
        string pickup,
        string ret,
        Money total,
        Money deposit)
    {
        return new RentalBooking
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BookingNumber = bookingNumber,
            RentalVehicleId = vehicleId,
            ClientId = clientId,
            StartDate = start,
            EndDate = end,
            PickupLocation = pickup,
            ReturnLocation = ret,
            TotalRentAmount = total,
            DepositAmount = deposit,
            Status = "Reserved",
            CreatedAtUtc = DateTime.UtcNow,
        };
    }
}
