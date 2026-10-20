using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.FleetRental;

public sealed class RentalBooking : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string BookingNumber { get; private set; } = string.Empty;
    public Guid RentalVehicleId { get; private set; }
    public Guid ClientId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string PickupLocation { get; private set; } = string.Empty;
    public string ReturnLocation { get; private set; } = string.Empty;
    public Money TotalRentAmount { get; private set; } = Money.Euro(0);
    public Money DepositAmount { get; private set; } = Money.Euro(0);

    /// <summary>
    /// Reserved, Active, Completed, Cancelled
    /// </summary>
    public string Status { get; private set; } = "Reserved";

    public DateTime CreatedAtUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }

    private RentalBooking()
    {
    }

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