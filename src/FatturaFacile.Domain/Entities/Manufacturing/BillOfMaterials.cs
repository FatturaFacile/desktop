// <copyright file="BillOfMaterials.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities.Manufacturing;

/// <summary>
/// Represents bill of materials.
/// </summary>
public sealed class BillOfMaterials : AggregateRoot<Guid>, ITenantEntity, IAuditableEntity
{

    /// <summary>
    /// The internal collection of bill of materials component lines.
    /// </summary>
    private readonly List<BomLine> lines = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="BillOfMaterials"/> class.
    /// </summary>
    private BillOfMaterials()
    {
    }

    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets the finished product item id.
    /// </summary>
    public Guid FinishedProductItemId { get; private set; }

    /// <summary>
    /// Gets the bom code.
    /// </summary>
    public string BomCode { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the revision number.
    /// </summary>
    public string RevisionNumber { get; private set; } = "1.0";

    /// <summary>
    /// Gets the base yield percentage.
    /// </summary>
    public decimal BaseYieldPercentage { get; } = 100m;

    /// <summary>
    /// Gets a value indicating whether active.
    /// </summary>
    public bool IsActive { get; } = true;

    /// <summary>
    /// Gets the lines.
    /// </summary>
    public IReadOnlyCollection<BomLine> Lines => lines.AsReadOnly();

    /// <summary>
    /// Gets or sets the created at utc.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the updated at utc.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the updated by.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Create.
    /// </summary>
    /// <param name="tenantId">The tenant id.</param>
    /// <param name="finishedItemId">The finished item id.</param>
    /// <param name="code">The code.</param>
    /// <param name="revision">The revision.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static BillOfMaterials Create(
        Guid   tenantId,
        Guid   finishedItemId,
        string code,
        string revision = "1.0")
    {
        return new BillOfMaterials
               {
                   Id                    = Guid.NewGuid(),
                   TenantId              = tenantId,
                   FinishedProductItemId = finishedItemId,
                   BomCode               = code?.Trim().ToUpperInvariant() ?? string.Empty,
                   RevisionNumber        = revision?.Trim()                ?? string.Empty,
                   CreatedAtUtc          = DateTime.UtcNow,
               };
    }

    /// <summary>
    /// Add Component.
    /// </summary>
    /// <param name="componentItemId">The component item id.</param>
    /// <param name="quantity">The quantity.</param>
    /// <param name="scrapRate">The scrap rate.</param>
    public void AddComponent(
        Guid    componentItemId,
        decimal quantity,
        decimal scrapRate = 0m) =>
        lines.Add(
                  item: new BomLine(
                                    bomId: Id,
                                    componentItemId: componentItemId,
                                    quantity: quantity,
                                    scrapRate: scrapRate));

}
