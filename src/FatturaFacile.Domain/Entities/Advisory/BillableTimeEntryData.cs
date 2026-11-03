// <copyright file="BillableTimeEntryData.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Domain.Entities.Advisory;

/// <summary>
/// Data transfer object holding activity, fee, and timing details required to create a new <see cref="BillableTimeEntry"/>.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="MatterId">The unique identifier of the associated client matter.</param>
/// <param name="UserId">The unique identifier of the fee earner performing the advisory activity.</param>
/// <param name="Date">The calendar date when the activity occurred.</param>
/// <param name="Minutes">The total duration recorded in minutes.</param>
/// <param name="Rate">The agreed hourly billing rate.</param>
/// <param name="Currency">The billing currency.</param>
/// <param name="Description">A detailed narrative description of the professional advisory task.</param>
public sealed record BillableTimeEntryData(
    Guid     TenantId,
    Guid     MatterId,
    Guid     UserId,
    DateTime Date,
    int      Minutes,
    decimal  Rate,
    Currency Currency,
    string   Description);
