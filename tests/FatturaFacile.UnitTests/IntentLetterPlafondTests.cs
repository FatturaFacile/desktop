// <copyright file="IntentLetterPlafondTests.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using Xunit;

namespace FatturaFacile.UnitTests;

/// <summary>
/// Tests of the plafond rules of the declarations of intent (art. 8 c. 1 lett. c DPR 633/1972).
/// </summary>
public sealed class IntentLetterPlafondTests
{
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly DateTime Day = new(2027, 5, 10, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly Dictionary<Guid, decimal> NoUsage = [];

    /// <summary>
    /// An invoice within the residual plafond of a valid letter is allowed and names the letter.
    /// </summary>
    [Fact]
    public void Evaluate_WithinThePlafond_IsAllowed()
    {
        var letter = Letter(plafond: 10000m);

        var result = IntentLetterPlafond.Evaluate([letter], NoUsage, Customer, Day, required: 4000m);

        Assert.True(result.Allowed);
        Assert.Equal(letter.Id, result.LetterId);
        Assert.Equal(letter.ProtocolNumber, result.ProtocolNumber);
        Assert.Equal(10000m, result.Residual);
        Assert.Null(result.BlockCode);
        Assert.False(result.NearExhaustion);
    }

    /// <summary>
    /// An invoice that would exceed the residual plafond is blocked, and the block names the best letter.
    /// </summary>
    [Fact]
    public void Evaluate_ExceedingThePlafond_IsBlocked()
    {
        var letter = Letter(plafond: 10000m);
        Dictionary<Guid, decimal> usage = new() { [letter.Id] = 7000m };

        var result = IntentLetterPlafond.Evaluate([letter], usage, Customer, Day, required: 3000.01m);

        Assert.False(result.Allowed);
        Assert.Equal(VatComplianceErrors.BlockPlafondExceeded, result.BlockCode);
        Assert.Equal(3000m, result.Residual);
        Assert.Equal(letter.Id, result.LetterId);
    }

    /// <summary>
    /// A customer without a valid letter on the date of the invoice is blocked: wrong customer, expired, not yet valid, issued
    /// letters and deleted ones do not count.
    /// </summary>
    [Fact]
    public void Evaluate_WithoutAValidLetter_IsBlocked()
    {
        var expired = Letter(validTo: Day.AddDays(-1));
        var future = Letter(validFrom: Day.AddDays(1));
        var issued = Letter();
        issued.Direction = IntentLetterDirection.Issued;
        var deleted = Letter();
        deleted.DeletedAt = Day;
        var pendingDelete = Letter();
        pendingDelete.SyncState = VatComplianceSyncState.PendingDelete;
        var other = Letter();
        other.ContactId = Guid.NewGuid();

        var result = IntentLetterPlafond.Evaluate([expired, future, issued, deleted, pendingDelete, other], NoUsage, Customer, Day, required: 1m);

        Assert.False(result.Allowed);
        Assert.Equal(VatComplianceErrors.BlockNoValidLetter, result.BlockCode);
        Assert.Null(result.LetterId);
    }

    /// <summary>
    /// The erosion is the larger of the server figure and the invoices issued here, so nothing is forgotten before the sync.
    /// </summary>
    [Fact]
    public void Consumed_TakesTheLargerOfServerAndLocalErosion()
    {
        var letter = Letter(plafond: 1000m);
        letter.ServerConsumedAmount = 300m;

        Assert.Equal(300m, IntentLetterPlafond.Consumed(letter, localUsage: 100m));
        Assert.Equal(450m, IntentLetterPlafond.Consumed(letter, localUsage: 450m));
        Assert.Equal(550m, IntentLetterPlafond.Residual(letter, localUsage: 450m));
        Assert.Equal(0m, IntentLetterPlafond.Residual(letter, localUsage: 5000m));
    }

    /// <summary>
    /// A second letter takes over when the first has not enough plafond left, and an invoice that leaves under a tenth is flagged.
    /// </summary>
    [Fact]
    public void Evaluate_UsesTheNextLetterAndFlagsNearExhaustion()
    {
        var first = Letter(plafond: 1000m);
        first.ReceiptDate = Day.AddDays(-30);
        var second = Letter(plafond: 5000m);
        Dictionary<Guid, decimal> usage = new() { [first.Id] = 900m };

        var taken = IntentLetterPlafond.Evaluate([first, second], usage, Customer, Day, required: 500m);
        var near = IntentLetterPlafond.Evaluate([first, second], usage, Customer, Day, required: 60m);

        Assert.Equal(second.Id, taken.LetterId);
        Assert.Equal(first.Id, near.LetterId);
        Assert.True(near.NearExhaustion);
    }

    private static IntentLetter Letter(decimal plafond = 100_000m, DateTime? validFrom = null, DateTime? validTo = null) => new()
    {
        ContactId = Customer,
        Direction = IntentLetterDirection.Received,
        ProtocolNumber = "08060120341234567-" + Guid.NewGuid().ToString("N")[..6],
        ReceiptDate = Day.AddDays(-10),
        Year = 2027,
        PlafondAmount = plafond,
        ValidFrom = validFrom ?? new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
        ValidTo = validTo ?? new DateTime(2027, 12, 31, 0, 0, 0, DateTimeKind.Unspecified),
        SyncState = VatComplianceSyncState.Synced,
    };
}
