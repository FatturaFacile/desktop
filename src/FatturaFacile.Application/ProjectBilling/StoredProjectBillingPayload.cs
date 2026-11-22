// <copyright file="StoredProjectBillingPayload.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.ProjectBilling;

/// <summary>
/// A stored billing payload and the instant it was pulled.
/// </summary>
/// <param name="Payload">The payload.</param>
/// <param name="SyncedAtUtc">When it was pulled.</param>
public sealed record StoredProjectBillingPayload(ProjectBillingPayload Payload, DateTime SyncedAtUtc);
