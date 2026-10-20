using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Manufacturing;

public sealed class WorkCenter : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal HourlyMachineCost { get; private set; }
    public decimal CapacityHoursPerDay { get; private set; } = 8.0m;

    private WorkCenter()
    {
    }

    public static WorkCenter Create(Guid tenantId, string code, string name, decimal cost, decimal capacityHours = 8.0m)
    {
        return new WorkCenter
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            HourlyMachineCost = cost,
            CapacityHoursPerDay = capacityHours,
        };
    }
}