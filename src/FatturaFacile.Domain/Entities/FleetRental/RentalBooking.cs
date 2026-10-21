// <copyright file="RentalBooking.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Represents rental booking.
/// </summary>
public sealed class RentalBooking : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    private RentalBooking()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the booking number.
    /// </summary>
    public string BookingNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the rental vehicle id.
    /// </summary>
    public Guid RentalVehicleId { get; private set; }

    /// <summary>
    /// Gets the client id.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the start date.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the end date.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Gets the pickup location.
    /// </summary>
    public string PickupLocation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the return location.
    /// </summary>
    public string ReturnLocation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the total rent amount.
    /// </summary>
    public Money TotalRentAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the deposit amount.
    /// </summary>
    public Money DepositAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the reserved, Active, Completed, Cancelled.
    /// </summary>
    public string Status { get; private set; } = "Reserved";

    /// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="bookingNumber">The booking number.</param>
    /// <param name="vehicleId">The vehicle id.</param>
    /// <param name="clientId">The client id.</param>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <param name="pickup">The pickup.</param>
    /// <param name="ret">The ret.</param>
    /// <param name="total">The total.</param>
    /// <param name="deposit">The deposit.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
