// <copyright file="VatComplianceSyncService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Sync;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// <see cref="IVatComplianceSyncService"/> over the repositories of the unit of work and the web-api client.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="time">The clock.</param>
public sealed class VatComplianceSyncService(IVatComplianceApiClient api, IUnitOfWork unitOfWork, TimeProvider time) : IVatComplianceSyncService
{
    private const int ErrorLength = 500;

    private static readonly Dictionary<HttpStatusCode, VatComplianceSyncStatus> ByStatus = new()
    {
        [HttpStatusCode.Unauthorized] = VatComplianceSyncStatus.Unauthorized,
        [HttpStatusCode.Forbidden] = VatComplianceSyncStatus.Forbidden,
    };

    /// <inheritdoc/>
    public Task<VatComplianceSyncResult> SyncAsync(int year, CancellationToken cancellationToken) =>
        api.IsAuthenticated ? RunAsync(year, cancellationToken) : Task.FromResult(new VatComplianceSyncResult(VatComplianceSyncStatus.Unauthorized));

    private static bool IsRefusal(SyncHttpReply reply) => reply.Status is HttpStatusCode.UnprocessableEntity or HttpStatusCode.Conflict or HttpStatusCode.BadRequest;

    private static HttpRequestException Failure(SyncHttpReply reply) =>
        new("VAT compliance request failed with HTTP " + ((int)reply.Status).ToString(System.Globalization.CultureInfo.InvariantCulture) + ".", inner: null, reply.Status);

    private static string Shorten(string body) => body.Length <= ErrorLength ? body : body[..ErrorLength];

    private async Task<VatComplianceSyncResult> RunAsync(int year, CancellationToken cancellationToken)
    {
        Tally tally = new();

        try
        {
            await PushLettersAsync(tally, cancellationToken).ConfigureAwait(false);
            await PushPaymentsAsync(tally, cancellationToken).ConfigureAwait(false);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            await PullLettersAsync(year, tally, cancellationToken).ConfigureAwait(false);
            await PullSettlementsAsync(year, tally, cancellationToken).ConfigureAwait(false);
            await PullWithholdingsAsync(year, tally, cancellationToken).ConfigureAwait(false);
            await PullProtocolsAsync(year, tally, cancellationToken).ConfigureAwait(false);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new VatComplianceSyncResult(VatComplianceSyncStatus.Synced, time.GetUtcNow().UtcDateTime, tally.Pushed, tally.Pulled, tally.Conflicts, tally.ProtocolsMatched);
        }
        catch (HttpRequestException ex)
        {
            // What was accepted before the failure is kept; no status code means the server never answered.
            _ = await unitOfWork.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            var status = ex.StatusCode is { } code ? ByStatus.GetValueOrDefault(code, VatComplianceSyncStatus.Failed) : VatComplianceSyncStatus.Offline;

            return new VatComplianceSyncResult(status, Pushed: tally.Pushed, Conflicts: tally.Conflicts, Detail: ex.Message);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or KeyNotFoundException)
        {
            return new VatComplianceSyncResult(VatComplianceSyncStatus.Failed, Detail: ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return new VatComplianceSyncResult(VatComplianceSyncStatus.Offline, Detail: ex.Message);
        }
    }

    private async Task PushLettersAsync(Tally tally, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<IntentLetter>();
        var pending = await repository.FindAsync(
            static l => l.SyncState == VatComplianceSyncState.PendingCreate || l.SyncState == VatComplianceSyncState.PendingUpdate || l.SyncState == VatComplianceSyncState.PendingDelete,
            cancellationToken).ConfigureAwait(false);

        foreach (var letter in pending)
        {
            if (letter.SyncState == VatComplianceSyncState.PendingDelete)
            {
                await PushDeleteAsync(letter, tally, cancellationToken).ConfigureAwait(false);
                continue;
            }

            var json = VatComplianceParser.ToIntentLetterJson(letter);
            var reply = letter.ServerId is { } serverId && letter.SyncState == VatComplianceSyncState.PendingUpdate
                ? await api.UpdateIntentLetterAsync(serverId, json, cancellationToken).ConfigureAwait(false)
                : await api.CreateIntentLetterAsync(json, cancellationToken).ConfigureAwait(false);

            if (reply.IsSuccess)
            {
                letter.ServerId ??= VatComplianceParser.ParseCreatedId(reply.Body);
                letter.SyncState = VatComplianceSyncState.Synced;
                letter.SyncError = null;
                tally.Pushed++;
            }
            else if (IsRefusal(reply))
            {
                letter.SyncState = VatComplianceSyncState.Conflict;
                letter.SyncError = Shorten(reply.Body);
                tally.Conflicts++;
            }
            else
            {
                throw Failure(reply);
            }

            repository.Update(letter);
        }
    }

    private async Task PushDeleteAsync(IntentLetter letter, Tally tally, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<IntentLetter>();
        if (letter.ServerId is not { } serverId)
        {
            repository.Delete(letter);

            return;
        }

        var reply = await api.DeleteIntentLetterAsync(serverId, cancellationToken).ConfigureAwait(false);
        if (reply.IsSuccess || reply.Status == HttpStatusCode.NotFound)
        {
            repository.Delete(letter);
            tally.Pushed++;
        }
        else if (IsRefusal(reply))
        {
            letter.SyncState = VatComplianceSyncState.Conflict;
            letter.SyncError = Shorten(reply.Body);
            letter.DeletedAt = null;
            repository.Update(letter);
            tally.Conflicts++;
        }
        else
        {
            throw Failure(reply);
        }
    }

    private async Task PushPaymentsAsync(Tally tally, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<WithholdingF24Payment>();
        var pending = await repository.FindAsync(static p => p.SyncState == VatComplianceSyncState.PendingUpdate && p.PaidDate != null, cancellationToken).ConfigureAwait(false);

        // One pay request settles all the tax codes of a month, so it is sent once the month is paid on every code.
        foreach (var settlement in pending.Where(static p => p.SettlementServerId is not null).GroupBy(static p => p.SettlementServerId!.Value))
        {
            var all = await repository.FindAsync(p => p.SettlementServerId == settlement.Key, cancellationToken).ConfigureAwait(false);
            if (all.Any(static p => p.PaidDate is null))
            {
                continue;
            }

            var reply = await api.PayWithholdingSettlementAsync(settlement.Key, VatComplianceParser.ToPaymentJson(all.Max(static p => p.PaidDate!.Value)), cancellationToken).ConfigureAwait(false);
            var accepted = reply.IsSuccess || reply.Status == HttpStatusCode.Conflict;
            if (!accepted && !IsRefusal(reply))
            {
                throw Failure(reply);
            }

            foreach (var payment in all)
            {
                payment.SyncState = accepted ? VatComplianceSyncState.Synced : VatComplianceSyncState.Conflict;
                payment.SyncError = accepted ? null : Shorten(reply.Body);
                repository.Update(payment);
            }

            if (accepted)
            {
                tally.Pushed++;
            }
            else
            {
                tally.Conflicts++;
            }
        }
    }

    private async Task PullLettersAsync(int year, Tally tally, CancellationToken cancellationToken)
    {
        var remote = VatComplianceParser.ParseIntentLetters(await api.GetIntentLettersAsync(year, cancellationToken).ConfigureAwait(false));
        var repository = unitOfWork.Repository<IntentLetter>();
        var local = await repository.FindAsync(l => l.Year == year, cancellationToken).ConfigureAwait(false);

        foreach (var incoming in remote)
        {
            tally.Pulled++;
            var existing = local.FirstOrDefault(l => l.ServerId == incoming.ServerId)
                ?? local.FirstOrDefault(l => l.ServerId is null && string.Equals(l.ProtocolNumber, incoming.ProtocolNumber, StringComparison.Ordinal));

            if (existing is null)
            {
                await repository.AddAsync(incoming, cancellationToken).ConfigureAwait(false);
                continue;
            }

            existing.ServerId = incoming.ServerId;
            existing.ServerConsumedAmount = incoming.ServerConsumedAmount;
            if (existing.SyncState == VatComplianceSyncState.Synced)
            {
                // A row without offline changes simply follows the server; the others keep their changes until they are sent.
                existing.ContactId = incoming.ContactId;
                existing.Direction = incoming.Direction;
                existing.ProtocolNumber = incoming.ProtocolNumber;
                existing.ReceiptDate = incoming.ReceiptDate;
                existing.PlafondAmount = incoming.PlafondAmount;
                existing.ValidFrom = incoming.ValidFrom;
                existing.ValidTo = incoming.ValidTo;
                existing.Notes = incoming.Notes;
                existing.DeletedAt = null;
            }

            repository.Update(existing);
        }

        // A synced letter the server no longer has was deleted there.
        var remoteIds = remote.Select(static l => l.ServerId).ToHashSet();
        foreach (var gone in local.Where(l => l.SyncState == VatComplianceSyncState.Synced && l.ServerId is { } id && l.DeletedAt is null && !remoteIds.Contains(id)))
        {
            gone.DeletedAt = time.GetUtcNow().UtcDateTime;
            repository.Update(gone);
        }
    }

    private async Task PullSettlementsAsync(int year, Tally tally, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var remote = VatComplianceParser.ParseSettlements(await api.GetSettlementsAsync(year, cancellationToken).ConfigureAwait(false), now);
        var repository = unitOfWork.Repository<VatSettlementSnapshot>();
        var local = await repository.FindAsync(s => s.Year == year, cancellationToken).ConfigureAwait(false);

        foreach (var incoming in remote)
        {
            tally.Pulled++;
            var existing = local.FirstOrDefault(s => s.ServerId == incoming.ServerId);
            if (existing is null)
            {
                await repository.AddAsync(incoming, cancellationToken).ConfigureAwait(false);
                continue;
            }

            existing.PeriodType = incoming.PeriodType;
            existing.Period = incoming.Period;
            existing.PeriodFrom = incoming.PeriodFrom;
            existing.PeriodTo = incoming.PeriodTo;
            existing.OutputVat = incoming.OutputVat;
            existing.InputVat = incoming.InputVat;
            existing.PreviousCredit = incoming.PreviousCredit;
            existing.PreviousDebit = incoming.PreviousDebit;
            existing.Interest = incoming.Interest;
            existing.PayableAmount = incoming.PayableAmount;
            existing.Status = incoming.Status;
            existing.SettlementJournalId = incoming.SettlementJournalId;
            existing.SyncedAtUtc = now;
            repository.Update(existing);
        }
    }

    private async Task PullWithholdingsAsync(int year, Tally tally, CancellationToken cancellationToken)
    {
        var ids = VatComplianceParser.ParseWithholdingSettlementIds(await api.GetWithholdingSettlementsAsync(year, cancellationToken).ConfigureAwait(false));
        var lines = unitOfWork.Repository<WithholdingLine>();
        var payments = unitOfWork.Repository<WithholdingF24Payment>();

        foreach (var id in ids)
        {
            var (incomingPayments, incomingLines) = VatComplianceParser.ParseWithholdingSettlement(await api.GetWithholdingSettlementAsync(id, cancellationToken).ConfigureAwait(false));
            tally.Pulled += incomingLines.Count;

            foreach (var old in await lines.FindAsync(l => l.SettlementServerId == id, cancellationToken).ConfigureAwait(false))
            {
                lines.Delete(old);
            }

            foreach (var line in incomingLines)
            {
                await lines.AddAsync(line, cancellationToken).ConfigureAwait(false);
            }

            var existing = await payments.FindAsync(p => p.SettlementServerId == id, cancellationToken).ConfigureAwait(false);
            foreach (var incoming in incomingPayments)
            {
                var current = existing.FirstOrDefault(p => string.Equals(p.TaxCode, incoming.TaxCode, StringComparison.Ordinal));
                if (current is null)
                {
                    await payments.AddAsync(incoming, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                current.Amount = incoming.Amount;
                current.DueDate = incoming.DueDate;
                if (current.SyncState == VatComplianceSyncState.Synced)
                {
                    current.PaidDate = incoming.PaidDate;
                }

                payments.Update(current);
            }
        }
    }

    private async Task PullProtocolsAsync(int year, Tally tally, CancellationToken cancellationToken)
    {
        var from = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var to = new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);
        var repository = unitOfWork.Repository<VatRegisterEntry>();

        foreach (var (type, kind) in new[] { ("sales", VatRegisterKind.Sales), ("purchases", VatRegisterKind.Purchases) })
        {
            var protocols = VatComplianceParser.ParseRegisterProtocols(await api.GetRegisterAsync(type, from, to, cancellationToken).ConfigureAwait(false));
            var entries = await repository.FindAsync(e => e.Kind == kind && e.Year == year, cancellationToken).ConfigureAwait(false);

            foreach (var entry in entries.Where(e => protocols.ContainsKey(e.DocumentNumber)))
            {
                var protocol = protocols[entry.DocumentNumber];
                if (entry.ServerProtocolNumber != protocol)
                {
                    entry.ServerProtocolNumber = protocol;
                    repository.Update(entry);
                    tally.ProtocolsMatched++;
                }
            }
        }
    }

    private sealed class Tally
    {
        public int Pushed { get; set; }

        public int Pulled { get; set; }

        public int Conflicts { get; set; }

        public int ProtocolsMatched { get; set; }
    }
}
