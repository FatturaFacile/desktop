// <copyright file="DeclarationOfConformity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Construction;

/// <summary>
/// Represents a statutory technical declaration of conformity (Di.Co. - Dichiarazione di Conformità under Italian DM 37/08
/// or Swiss NIV / OIBT - Ordonnance sur les installations électriques à basse tension),
/// certifying that installed building systems comply with safety and engineering standards.
/// </summary>
public sealed class DeclarationOfConformity : Entity<Guid>, ITenantEntity
{
    private DeclarationOfConformity()
    {
    }

    /// <summary>
    /// Gets or sets the unique identifier of the tenant contractor issuing this declaration of conformity.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the unique identifier of the construction site where the installation was realized.
    /// </summary>
    public Guid ConstructionSiteId { get; private set; }

    /// <summary>
    /// Gets the formal certificate sequence or registration number.
    /// </summary>
    public string CertificateNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the statutory standard reference cited (e.g., Italian Ministerial Decree "DM 37/08" or Swiss "NIV / OIBT").
    /// </summary>
    public string StandardReference { get; private set; } = "DM 37/08";

    /// <summary>
    /// Gets the name and title of the certified certifying engineer or qualified technical director (Responsabile Tecnico).
    /// </summary>
    public string DeclarantEngineerName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date on which the conformity certificate was signed and issued.
    /// </summary>
    public DateTime IssueDate { get; private set; }

    /// <summary>
    /// Gets the classification and description of the technical system installed (e.g., electrical installation, HVAC, plumbing/heating, gas line).
    /// </summary>
    public string SystemTypeDescription { get; private set; } = string.Empty;

    /// <summary>
    /// Creates a statutory declaration of conformity certificate for an installed building plant or engineering system.
    /// </summary>
    /// <param name="tenantId">The unique tenant identifier.</param>
    /// <param name="siteId">The construction site identifier where works were executed.</param>
    /// <param name="certNum">The official certificate reference number.</param>
    /// <param name="standard">The statutory norm or regulatory reference (e.g., DM 37/08, NIV/OIBT).</param>
    /// <param name="engineer">The certified technical manager or engineer signing the attestation.</param>
    /// <param name="issueDate">The certificate issuance date.</param>
    /// <param name="systemType">The functional system category and scope description.</param>
    /// <returns>A newly created <see cref="DeclarationOfConformity"/> entity.</returns>
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
            CertificateNumber = certNum?.Trim() ?? string.Empty,
            StandardReference = standard?.Trim() ?? string.Empty,
            DeclarantEngineerName = engineer?.Trim() ?? string.Empty,
            IssueDate = issueDate,
            SystemTypeDescription = systemType?.Trim() ?? string.Empty,
        };
    }
}
