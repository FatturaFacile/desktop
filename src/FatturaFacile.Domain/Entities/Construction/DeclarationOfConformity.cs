using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Construction;

public sealed class DeclarationOfConformity : Entity<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ConstructionSiteId { get; private set; }
    public string CertificateNumber { get; private set; } = string.Empty;

    /// <summary>
    /// IT: DM 37/08; CH: NIV / OIBT
    /// </summary>
    public string StandardReference { get; private set; } = "DM 37/08";
    public string DeclarantEngineerName { get; private set; } = string.Empty;
    public DateTime IssueDate { get; private set; }

    /// <summary>
    /// Impianto elettrico, termoidraulico
    /// </summary>
    public string SystemTypeDescription { get; private set; } = string.Empty;

    private DeclarationOfConformity()
    {
    }

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