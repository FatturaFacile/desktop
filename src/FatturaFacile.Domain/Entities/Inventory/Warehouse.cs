using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Inventory;

public sealed class Warehouse : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public decimal TotalCapacitySquareMeters { get; private set; }

    private Warehouse()
    {
    }

    public static Warehouse Create(Guid tenantId, string code, string name, Address address, decimal capacity)
    {
        return new Warehouse
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Address = address,
            TotalCapacitySquareMeters = capacity,
        };
    }
}