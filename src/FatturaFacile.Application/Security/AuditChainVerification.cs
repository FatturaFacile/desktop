// <copyright file="AuditChainVerification.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Security;

/// <summary>
/// Result of verifying the hash chain.
/// </summary>
/// <param name="IsValid">Whether every entry is intact and correctly linked.</param>
/// <param name="EntriesChecked">The number of entries examined.</param>
/// <param name="BrokenAtSequence">The sequence number of the first bad entry, if any.</param>
/// <param name="Reason">A diagnostic code of the failure (for example "record_hash_mismatch").</param>
public sealed record AuditChainVerification(bool IsValid, int EntriesChecked, long? BrokenAtSequence, string? Reason);
