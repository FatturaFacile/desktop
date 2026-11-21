// <copyright file="AuditChainVerifier.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Security;

/// <summary>
/// Verifies the integrity of the audit chain: contiguous sequence, links, payload hashes and HMAC signatures.
/// </summary>
public static class AuditChainVerifier
{
    /// <summary>
    /// Verifies a chain.
    /// </summary>
    /// <param name="entries">The entries ordered by sequence number.</param>
    /// <param name="key">The device key the chain was signed with.</param>
    /// <returns>The verification result (stops at the first bad entry).</returns>
    public static AuditChainVerification Verify(IEnumerable<LocalAuditEntry> entries, byte[] key)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(key);

        var checkedCount = 0;
        long expectedSequence = 1;
        var expectedPrevious = AuditChain.GenesisHash;

        foreach (var entry in entries)
        {
            var reason = Validate(entry, expectedSequence, expectedPrevious, key);
            if (reason is not null)
            {
                return new AuditChainVerification(IsValid: false, checkedCount, entry.SequenceNumber, reason);
            }

            checkedCount++;
            expectedSequence++;
            expectedPrevious = entry.RecordHash;
        }

        return new AuditChainVerification(IsValid: true, checkedCount, BrokenAtSequence: null, Reason: null);
    }

    private static string? Validate(LocalAuditEntry entry, long expectedSequence, string expectedPrevious, byte[] key)
    {
        if (entry.SequenceNumber != expectedSequence)
        {
            return "sequence_gap";
        }

        if (!string.Equals(entry.PreviousHash, expectedPrevious, StringComparison.Ordinal))
        {
            return "previous_hash_mismatch";
        }

        if (!string.Equals(AuditChain.ComputePayloadHash(entry.PayloadJson), entry.PayloadHash, StringComparison.Ordinal))
        {
            return "payload_hash_mismatch";
        }

        var metadata = AuditChain.BuildMetadata(entry.SequenceNumber, entry.OccurredAt, entry.EntityType, entry.EntityId, entry.Action, entry.DeviceId);
        var expectedRecord = AuditChain.ComputeRecordHash(key, entry.PreviousHash, entry.PayloadHash, metadata);

        return CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expectedRecord), Encoding.ASCII.GetBytes(entry.RecordHash))
            ? null
            : "record_hash_mismatch";
    }
}
