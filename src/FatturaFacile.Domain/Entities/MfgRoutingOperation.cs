// <copyright file="MfgRoutingOperation.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// An operation step of a production routing.
/// </summary>
public class MfgRoutingOperation : ITenantScopedEntity
{
    /// <summary>
    /// Gets or sets the server identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the owning tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the local creation instant in UTC.
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the local update instant in UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the routing identifier.
    /// </summary>
    public Guid RoutingId { get; set; }

    /// <summary>
    /// Gets or sets the sequence.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// Gets or sets the operation code.
    /// </summary>
    public string OperationCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the work centre identifier.
    /// </summary>
    public Guid WorkCenterId { get; set; }

    /// <summary>
    /// Gets or sets the machine identifier.
    /// </summary>
    public Guid? MachineCenterId { get; set; }

    /// <summary>
    /// Gets or sets the setup time in minutes.
    /// </summary>
    public decimal SetupTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the run time per unit in minutes.
    /// </summary>
    public decimal RunTimeMinutesPerUnit { get; set; }

    /// <summary>
    /// Gets or sets the labour time per unit in minutes.
    /// </summary>
    public decimal LaborTimeMinutesPerUnit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the step is subcontracted.
    /// </summary>
    public bool IsSubcontracted { get; set; }

    /// <summary>
    /// Gets or sets the subcontractor contact identifier.
    /// </summary>
    public Guid? SubcontractorContactId { get; set; }

    /// <summary>
    /// Gets or sets the subcontracting cost per unit.
    /// </summary>
    public decimal SubcontractUnitCost { get; set; }

    /// <summary>
    /// Gets or sets the quality plan identifier.
    /// </summary>
    public Guid? QualityPlanId { get; set; }

    /// <summary>
    /// Gets or sets the operation type (internal_machining, manual_assembly, external_subcontracting).
    /// </summary>
    public string OperationType { get; set; } = "internal_machining";

    /// <summary>
    /// Gets or sets the queue time in minutes.
    /// </summary>
    public decimal QueueTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the wait after the operation in minutes.
    /// </summary>
    public decimal PostOperationWaitMinutes { get; set; }

    /// <summary>
    /// Gets or sets the move time in minutes.
    /// </summary>
    public decimal MoveTimeMinutes { get; set; }

    /// <summary>
    /// Gets or sets the transfer batch for operation overlap.
    /// </summary>
    public decimal? OverlapTransferBatchQty { get; set; }

    /// <summary>
    /// Gets or sets the number of operators.
    /// </summary>
    public int RequiredOperators { get; set; } = 1;

    /// <summary>
    /// Gets or sets the alternate work centre identifiers as a JSON array.
    /// </summary>
    public string AlternateWorkCenterIdsJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets the linked quality control point identifier.
    /// </summary>
    public Guid? LinkedQualityControlPointId { get; set; }

    /// <summary>
    /// Gets or sets the work instruction document identifier.
    /// </summary>
    public Guid? WorkInstructionDocId { get; set; }

    /// <summary>
    /// Gets or sets the bill of materials components consumed by the step.
    /// </summary>
    public ICollection<MfgRoutingOperationBomLink> BomComponents { get; set; } = [];
}
