// <copyright file="ProjectPendingChange.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Projects;

/// <summary>
/// A queued or refused project change, for the pending list.
/// </summary>
/// <param name="EntryId">The outbox entry identifier.</param>
/// <param name="Action">The action.</param>
/// <param name="ProjectId">The project identifier.</param>
/// <param name="ProjectCode">The project code.</param>
/// <param name="OccurredAtUtc">The instant (UTC) of the change.</param>
/// <param name="Error">The last error, if any.</param>
public sealed record ProjectPendingChange(Guid EntryId, ProjectOutboxAction Action, Guid ProjectId, string ProjectCode, DateTime OccurredAtUtc, string? Error);
