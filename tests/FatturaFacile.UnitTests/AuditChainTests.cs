// <copyright file="AuditChainTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using FatturaFacile.Application.Security;
using FatturaFacile.Domain.Entities;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the audit chain hashing and of tamper detection.
/// </summary>
public sealed class AuditChainTests
{
    private static readonly byte[] Key = [.. Enumerable.Range(1, 32).Select(static i => (byte)i)];
    private static readonly Guid Tenant = new(0x11111111, 0x1111, 0x1111, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11);
    private static readonly DateTime Start = new(2026, 11, 21, 8, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Canonical JSON sorts keys and drops whitespace, so equal content always hashes equally.
    /// </summary>
    [Fact]
    public void CanonicalJson_IsOrderAndWhitespaceIndependent()
    {
        var a = AuditChain.ToCanonicalJson("""{ "b": 1, "a": { "z": true, "y": [3, 2] } }""");
        var b = AuditChain.ToCanonicalJson(new Dictionary<string, object?>(StringComparer.Ordinal) { ["a"] = new Dictionary<string, object?>(StringComparer.Ordinal) { ["y"] = new[] { 3, 2 }, ["z"] = true }, ["b"] = 1 });

        Assert.Equal("""{"a":{"y":[3,2],"z":true},"b":1}""", a);
        Assert.Equal(a, b);
        Assert.Equal(AuditChain.ComputePayloadHash(a), AuditChain.ComputePayloadHash(b));
        Assert.Equal("null", AuditChain.ToCanonicalJson(value: null));
    }

    /// <summary>
    /// The payload hash is the lowercase hex SHA-256 of the canonical JSON.
    /// </summary>
    [Fact]
    public void PayloadHash_IsSha256OfCanonicalJson() =>
        Assert.Equal("74234e98afe7498fb5daf1f36ac2d78acc339464f950703b8c019892f982b90b", AuditChain.ComputePayloadHash("null"));

    /// <summary>
    /// The first entry links to the genesis hash and every record hash is a 64-character HMAC.
    /// </summary>
    [Fact]
    public void Create_FirstEntry_LinksToGenesis()
    {
        var entry = AuditEntryFactory.Create(previous: null, Input("create"), Key, "device-1", Start, Tenant);

        Assert.Equal(1, entry.SequenceNumber);
        Assert.Equal(AuditChain.GenesisHash, entry.PreviousHash);
        Assert.Equal(64, entry.RecordHash.Length);
        Assert.Equal(AuditChain.ComputePayloadHash(entry.PayloadJson), entry.PayloadHash);
        Assert.Equal(DateTimeKind.Utc, entry.OccurredAt.Kind);
    }

    /// <summary>
    /// The record hash is bound to the device key: another key produces another hash.
    /// </summary>
    [Fact]
    public void RecordHash_DependsOnTheDeviceKey()
    {
        var one = AuditEntryFactory.Create(previous: null, Input("create"), Key, "device-1", Start, Tenant);
        var other = AuditEntryFactory.Create(previous: null, Input("create"), RandomNumberGenerator.GetBytes(32), "device-1", Start, Tenant);

        Assert.NotEqual(one.RecordHash, other.RecordHash, StringComparer.Ordinal);
    }

    /// <summary>
    /// An untouched chain verifies, including the empty chain.
    /// </summary>
    [Fact]
    public void Verify_IntactChain_IsValid()
    {
        var result = AuditChainVerifier.Verify(BuildChain(5), Key);

        Assert.True(result.IsValid);
        Assert.Equal(5, result.EntriesChecked);
        Assert.Null(result.BrokenAtSequence);
        Assert.True(AuditChainVerifier.Verify([], Key).IsValid);
    }

    /// <summary>
    /// Editing the payload of an entry is detected at that entry.
    /// </summary>
    [Fact]
    public void Verify_TamperedPayload_IsDetected()
    {
        var chain = BuildChain(4);
        chain[2].PayloadJson = """{"amount":999}""";

        var result = AuditChainVerifier.Verify(chain, Key);

        Assert.False(result.IsValid);
        Assert.Equal(3, result.BrokenAtSequence);
        Assert.Equal("payload_hash_mismatch", result.Reason);
    }

    /// <summary>
    /// A forged payload with a recomputed payload hash still fails the HMAC (the attacker lacks the key).
    /// </summary>
    [Fact]
    public void Verify_PayloadAndHashRewritten_FailsTheSignature()
    {
        var chain = BuildChain(3);
        chain[1].PayloadJson = """{"amount":1}""";
        chain[1].PayloadHash = AuditChain.ComputePayloadHash(chain[1].PayloadJson);

        var result = AuditChainVerifier.Verify(chain, Key);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.BrokenAtSequence);
        Assert.Equal("record_hash_mismatch", result.Reason);
    }

    /// <summary>
    /// Changing signed metadata (action, entity, time, device) is detected.
    /// </summary>
    /// <param name="field">The tampered metadata field.</param>
    [Theory]
    [InlineData("action")]
    [InlineData("entity")]
    [InlineData("time")]
    [InlineData("device")]
    public void Verify_TamperedMetadata_IsDetected(string field)
    {
        var chain = BuildChain(3);
        var target = chain[1];
        switch (field)
        {
            case "action":
                target.Action = "delete";
                break;
            case "entity":
                target.EntityId = "other";
                break;
            case "time":
                target.OccurredAt = target.OccurredAt.AddMinutes(-5);
                break;
            default:
                target.DeviceId = "device-2";
                break;
        }

        var result = AuditChainVerifier.Verify(chain, Key);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.BrokenAtSequence);
        Assert.Equal("record_hash_mismatch", result.Reason);
    }

    /// <summary>
    /// A removed entry breaks the sequence; swapped entries break the links.
    /// </summary>
    [Fact]
    public void Verify_RemovedOrReorderedEntries_AreDetected()
    {
        var chain = BuildChain(4);

        var removed = AuditChainVerifier.Verify(chain.Where(static e => e.SequenceNumber != 2), Key);
        var reordered = AuditChainVerifier.Verify([chain[0], chain[2], chain[1], chain[3]], Key);

        Assert.Equal("sequence_gap", removed.Reason);
        Assert.Equal(3, removed.BrokenAtSequence);
        Assert.False(reordered.IsValid);
    }

    /// <summary>
    /// A broken link is reported even when the entry itself is internally consistent.
    /// </summary>
    [Fact]
    public void Verify_WrongPreviousHash_IsDetected()
    {
        var chain = BuildChain(3);
        chain[2].PreviousHash = chain[0].RecordHash;

        var result = AuditChainVerifier.Verify(chain, Key);

        Assert.Equal("previous_hash_mismatch", result.Reason);
        Assert.Equal(3, result.BrokenAtSequence);
    }

    /// <summary>
    /// A chain verified with the wrong key fails at the first entry.
    /// </summary>
    [Fact]
    public void Verify_WrongKey_FailsAtFirstEntry()
    {
        var result = AuditChainVerifier.Verify(BuildChain(2), RandomNumberGenerator.GetBytes(32));

        Assert.False(result.IsValid);
        Assert.Equal(1, result.BrokenAtSequence);
    }

    private static AuditEventInput Input(string action) =>
        new("contacts", "c-1", action, new Dictionary<string, object?>(StringComparer.Ordinal) { ["amount"] = 10, ["note"] = "hello" });

    private static List<LocalAuditEntry> BuildChain(int count)
    {
        List<LocalAuditEntry> chain = [];
        LocalAuditEntry? previous = null;
        for (var i = 0; i < count; i++)
        {
            previous = AuditEntryFactory.Create(previous, Input("update"), Key, "device-1", Start.AddMinutes(i), Tenant);
            chain.Add(previous);
        }

        return chain;
    }
}
