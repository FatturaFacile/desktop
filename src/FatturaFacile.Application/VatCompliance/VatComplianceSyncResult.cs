// <copyright file="VatComplianceSyncResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// The outcome of a sync of the VAT compliance data.
/// </summary>
/// <param name="Status">How the attempt ended.</param>
/// <param name="SyncedAtUtc">When the attempt ended, for a successful one.</param>
/// <param name="Pushed">How many offline changes the server accepted.</param>
/// <param name="Pulled">How many rows were read from the server.</param>
/// <param name="Conflicts">How many offline changes the server refused.</param>
/// <param name="ProtocolsMatched">How many register documents got the protocol number of the server.</param>
/// <param name="Detail">The technical detail of a failure.</param>
public sealed record VatComplianceSyncResult(VatComplianceSyncStatus Status, DateTime? SyncedAtUtc = null, int Pushed = 0, int Pulled = 0, int Conflicts = 0, int ProtocolsMatched = 0, string? Detail = null);
