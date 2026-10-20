using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Manufacturing;

/// <summary>
/// Represents work center.
/// </summary>
public sealed class WorkCenter : Entity<Guid>, ITenantEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the code.
    /// </summary>
    public string Code { get; private set; } = string.Empty;

/// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

/// <summary>
    /// Gets the hourly machine cost.
    /// </summary>
    public decimal HourlyMachineCost { get; private set; }

/// <summary>
    /// Gets the capacity hours per day.
    /// </summary>
    public decimal CapacityHoursPerDay { get; private set; } = 8.0m;

private WorkCenter()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="code">The code.</param>
    /// <param name="name">The name.</param>
    /// <param name="cost">The cost.</param>
    /// <param name="capacityHours">The capacity hours.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
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
