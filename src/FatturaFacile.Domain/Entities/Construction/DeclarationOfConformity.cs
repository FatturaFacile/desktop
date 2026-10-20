using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Represents declaration of conformity.
/// </summary>
public sealed class DeclarationOfConformity : Entity<Guid>, ITenantEntity
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

/// <summary>
    /// Gets the construction site id.
    /// </summary>
    public Guid ConstructionSiteId { get; private set; }

/// <summary>
    /// Gets the certificate number.
    /// </summary>
    public string CertificateNumber { get; private set; } = string.Empty;

/// <summary>
    /// Gets the iT: DM 37/08; CH: NIV / OIBT.
    /// </summary>
    public string StandardReference { get; private set; } = "DM 37/08";

/// <summary>
    /// Gets the declarant engineer name.
    /// </summary>
    public string DeclarantEngineerName { get; private set; } = string.Empty;

/// <summary>
    /// Gets the issue date.
    /// </summary>
    public DateTime IssueDate { get; private set; }

/// <summary>
    /// Gets the impianto elettrico, termoidraulico.
    /// </summary>
    public string SystemTypeDescription { get; private set; } = string.Empty;

private DeclarationOfConformity()
    {
    }

/// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="siteId">The site id.</param>
    /// <param name="certNum">The cert num.</param>
    /// <param name="standard">The standard.</param>
    /// <param name="engineer">The engineer.</param>
    /// <param name="issueDate">The issue date.</param>
    /// <param name="systemType">The system type.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static DeclarationOfConformity Create(
        Guid tenantId,
        Guid siteId,
        string certNum,
        string standard,
        string engineer,
        DateTime issueDate,
        string systemType)
    {
        return new DeclarationOfConformity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ConstructionSiteId = siteId,
            CertificateNumber = certNum.Trim(),
            StandardReference = standard.Trim(),
            DeclarantEngineerName = engineer.Trim(),
            IssueDate = issueDate,
            SystemTypeDescription = systemType.Trim(),
        };
    }
}
