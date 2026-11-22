// <copyright file="FlushCustomerActivityOutboxCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.CustomerLifecycle;

/// <summary>
/// Handler for <see cref="FlushCustomerActivityOutboxCommand"/>. Stops at the first unreachable answer to keep the order; an
/// entry the server refuses is marked as a conflict and skipped.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="outbox">The local outbox.</param>
/// <param name="time">The clock.</param>
public sealed class FlushCustomerActivityOutboxCommandHandler(ICustomerLifecycleApiClient api, ICustomerActivityOutbox outbox, TimeProvider time) : IRequestHandler<FlushCustomerActivityOutboxCommand, int>
{
    private const int MaxBackoffMinutes = 30;
    private const int MaxBackoffShift = 5;

    /// <inheritdoc/>
    public Task<int> Handle(FlushCustomerActivityOutboxCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return FlushAsync(cancellationToken);
    }

    private async Task<int> FlushAsync(CancellationToken cancellationToken)
    {
        var entries = await outbox.PendingAsync(cancellationToken).ConfigureAwait(false);
        var sent = 0;

        foreach (var entry in entries)
        {
            var now = time.GetUtcNow().UtcDateTime;
            if (entry.NextAttemptAt > now)
            {
                break;
            }

            try
            {
                await api.CreateActivityAsync(entry.AccountId, entry.Type, entry.Subject, entry.DueDate, cancellationToken).ConfigureAwait(false);
                await outbox.RemoveAsync(entry, cancellationToken).ConfigureAwait(false);
                sent++;
            }
            catch (Exception ex) when (CustomerActivityConnectivity.IsUnreachable(ex, cancellationToken))
            {
                entry.Attempts++;
                entry.LastError = ex.Message;
                entry.NextAttemptAt = now.AddMinutes(Math.Min(MaxBackoffMinutes, 1 << Math.Min(entry.Attempts, MaxBackoffShift)));
                await outbox.SaveAsync(cancellationToken).ConfigureAwait(false);

                break;
            }
            catch (HttpRequestException ex)
            {
                entry.Status = CustomerActivityOutboxStatus.Conflict;
                entry.LastError = ex.Message;
                await outbox.SaveAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        return sent;
    }
}
