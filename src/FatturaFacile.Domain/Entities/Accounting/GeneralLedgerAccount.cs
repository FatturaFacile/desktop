using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Accounting;

/// <summary>
/// Represents general ledger account.
/// </summary>
public sealed class GeneralLedgerAccount : Entity<Guid>, ITenantEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the swiss KMU: &amp;quot;1020&amp;quot;, IT: &amp;quot;01.01.001&amp;quot;.
    /// </summary>
    public string AccountCode { get; private set; } = string.Empty;

/// <summary>
    /// Gets the name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

/// <summary>
    /// Gets the category.
    /// </summary>
    public AccountCategory Category { get; private set; }

/// <summary>
    /// Gets the jurisdiction.
    /// </summary>
    public Jurisdiction Jurisdiction { get; private set; }

/// <summary>
    /// Gets a value indicating whether active.
    /// </summary>
    public bool IsActive { get; private set; } = true;

private GeneralLedgerAccount()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="code">The code.</param>
    /// <param name="name">The name.</param>
    /// <param name="category">The category.</param>
    /// <param name="jurisdiction">The jurisdiction.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static GeneralLedgerAccount Create(
        Guid tenantId,
        string code,
        string name,
        AccountCategory category,
        Jurisdiction jurisdiction)
    {
        return new GeneralLedgerAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            AccountCode = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Category = category,
            Jurisdiction = jurisdiction,
            IsActive = true,
        };
    }
}
