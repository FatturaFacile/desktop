// <copyright file="RentalBooking.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

/// <summary>
/// Represents a vehicle rental contract booking or reservation,
/// including vehicle assignment, rental period,
/// customer information and booking lifecycle state.
/// </summary>
public sealed class RentalBooking : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RentalBooking"/> class.
    /// Required by Entity Framework.
    /// </summary>
    private RentalBooking()
    {
    }

    /// <summary>
    /// Gets or sets the tenant identifier that owns the rental booking.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique booking reference number.
    /// </summary>
    public string BookingNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the rented vehicle.
    /// </summary>
    public Guid RentalVehicleId { get; private set; }

    /// <summary>
    /// Gets the identifier of the customer who made the booking.
    /// </summary>
    public Guid ClientId { get; private set; }

    /// <summary>
    /// Gets the rental start date and time.
    /// </summary>
    public DateTime StartDate { get; private set; }

    /// <summary>
    /// Gets the rental end date and time.
    /// </summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// Gets the location where the vehicle will be collected.
    /// </summary>
    public string PickupLocation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the location where the vehicle will be returned.
    /// </summary>
    public string ReturnLocation { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the total rental amount agreed for the booking.
    /// </summary>
    public Money TotalRentAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the security deposit amount associated with the booking.
    /// </summary>
    public Money DepositAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Gets the current booking status.
    /// </summary>
    public string Status { get; private set; } = RentalBookingStatus.Reserved;

    /// <summary>
    /// Gets or sets the UTC timestamp when the booking was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user or process that created the booking.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the booking was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the user or process that last modified the booking.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Creates a new vehicle rental booking.
    /// </summary>
    /// <param name="tenantId">
    /// The tenant identifier.
    /// </param>
    /// <param name="data">
    /// The booking data used to initialize the rental booking.
    /// </param>
    /// <returns>
    /// A configured <see cref="RentalBooking"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="data"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the rental end date is not greater than the start date.
    /// </exception>
    public static RentalBooking Create(
        Guid tenantId,
        RentalBookingData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return data.EndDate <= data.StartDate
            ? throw new ArgumentException(
                "End date must be greater than start date.",
                nameof(data))
            : new RentalBooking
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BookingNumber = data.BookingNumber.Trim(),
            RentalVehicleId = data.RentalVehicleId,
            ClientId = data.ClientId,
            StartDate = data.StartDate,
            EndDate = data.EndDate,
            PickupLocation = data.PickupLocation.Trim(),
            ReturnLocation = data.ReturnLocation.Trim(),
            TotalRentAmount = data.TotalRentAmount,
            DepositAmount = data.DepositAmount,
            Status = RentalBookingStatus.Reserved,
            CreatedAtUtc = DateTime.UtcNow,
        };
    }

    /// <summary>
    /// Marks the booking as active and indicates
    /// that the vehicle has been handed over to the customer.
    /// </summary>
    public void Activate() => Status = RentalBookingStatus.Active;

    /// <summary>
    /// Marks the booking as completed and indicates
    /// that the vehicle has been successfully returned.
    /// </summary>
    public void Complete() => Status = RentalBookingStatus.Completed;

    /// <summary>
    /// Marks the booking as cancelled.
    /// </summary>
    public void Cancel() => Status = RentalBookingStatus.Cancelled;
}
