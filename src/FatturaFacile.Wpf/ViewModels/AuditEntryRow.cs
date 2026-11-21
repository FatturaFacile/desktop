// <copyright file="AuditEntryRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// A display row of the offline audit queue grid.
/// </summary>
/// <param name="Sequence">The position in the chain.</param>
/// <param name="OccurredAt">The event instant (UTC), formatted.</param>
/// <param name="Entity">The entity type and identifier.</param>
/// <param name="Action">The action.</param>
/// <param name="Status">The localized delivery state.</param>
/// <param name="ServerReceivedAt">The server timestamp (UTC), formatted, or empty.</param>
internal sealed record AuditEntryRow(long Sequence, string OccurredAt, string Entity, string Action, string Status, string ServerReceivedAt);
