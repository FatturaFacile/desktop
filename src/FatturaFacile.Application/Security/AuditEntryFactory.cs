// <copyright file="AuditEntryFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Security;

/// <summary>
/// Builds signed entries of the audit chain.
/// </summary>
public static class AuditEntryFactory
{
    /// <summary>
    /// Creates the next entry of the chain.
    /// </summary>
    /// <param name="previous">The last entry of the chain, or null for the first one.</param>
    /// <param name="input">The event.</param>
    /// <param name="key">The device key.</param>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="occurredAtUtc">The occurrence instant (UTC).</param>
    /// <param name="tenantId">The tenant the entry belongs to.</param>
    /// <returns>The sealed entry (status pending).</returns>
    /// <exception cref="ArgumentException">The entity type or the action is empty.</exception>
    public static LocalAuditEntry Create(LocalAuditEntry? previous, AuditEventInput input, byte[] key, string deviceId, DateTime occurredAtUtc, Guid tenantId)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);

        if (string.IsNullOrWhiteSpace(input.EntityType) || string.IsNullOrWhiteSpace(input.Action))
        {
            throw new ArgumentException("Entity type and action are required.", nameof(input));
        }

        var sequence = (previous?.SequenceNumber ?? 0) + 1;
        var previousHash = previous?.RecordHash ?? AuditChain.GenesisHash;
        var payload = AuditChain.ToCanonicalJson(input.Payload);
        var payloadHash = AuditChain.ComputePayloadHash(payload);
        var occurredAt = DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc);
        var metadata = AuditChain.BuildMetadata(sequence, occurredAt, input.EntityType, input.EntityId, input.Action, deviceId);

        return new LocalAuditEntry
        {
            TenantId = tenantId,
            SequenceNumber = sequence,
            PreviousHash = previousHash,
            PayloadHash = payloadHash,
            PayloadJson = payload,
            RecordHash = AuditChain.ComputeRecordHash(key, previousHash, payloadHash, metadata),
            OccurredAt = occurredAt,
            EntityType = input.EntityType,
            EntityId = input.EntityId,
            Action = input.Action,
            DeviceId = deviceId,
        };
    }
}
