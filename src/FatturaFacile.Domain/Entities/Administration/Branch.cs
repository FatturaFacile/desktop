using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.ValueObjects;

namespace FatturaFacile.Domain.Entities.Administration;

public sealed class Branch : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string BranchCode { get; } = string.Empty;
    public string Name { get; } = string.Empty;
    public Address Address { get; } = null!;
    public bool IsHeadquarters { get; }

    internal Branch(Guid tenantId, string code, string name, Address address, bool isHq)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        BranchCode = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Address = address;
        IsHeadquarters = isHq;
    }

    private Branch()
    {
    }
}