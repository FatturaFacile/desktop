// <copyright file="CreateCustomerActivityCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.CustomerLifecycle;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.CustomerLifecycle;

/// <summary>
/// Handler for <see cref="CreateCustomerActivityCommand"/>. A refusal of the server (validation, authorization) is raised to the
/// caller and never queued; only an unreachable server saves the activity locally.
/// </summary>
/// <param name="api">The web-api client.</param>
/// <param name="outbox">The local outbox.</param>
/// <param name="tenant">The active tenant.</param>
/// <param name="time">The clock.</param>
public sealed class CreateCustomerActivityCommandHandler(ICustomerLifecycleApiClient api, ICustomerActivityOutbox outbox, ICurrentTenantService tenant, TimeProvider time) : IRequestHandler<CreateCustomerActivityCommand, CustomerActivityOutcome>
{
    /// <inheritdoc/>
    public Task<CustomerActivityOutcome> Handle(CreateCustomerActivityCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return SendOrQueueAsync(request, cancellationToken);
    }

    private async Task<CustomerActivityOutcome> SendOrQueueAsync(CreateCustomerActivityCommand request, CancellationToken cancellationToken)
    {
        var subject = request.Subject.Trim();

        try
        {
            await api.CreateActivityAsync(request.AccountId, request.Type, subject, request.DueDate, cancellationToken).ConfigureAwait(false);

            return CustomerActivityOutcome.Sent;
        }
        catch (Exception ex) when (CustomerActivityConnectivity.IsUnreachable(ex, cancellationToken))
        {
            var now = time.GetUtcNow().UtcDateTime;
            await outbox.EnqueueAsync(
                new CustomerActivityOutboxEntry
                {
                    TenantId = tenant.TenantId,
                    AccountId = request.AccountId,
                    Type = request.Type,
                    Subject = subject,
                    DueDate = request.DueDate,
                    OccurredAtUtc = now,
                    NextAttemptAt = now,
                    LastError = ex.Message,
                },
                cancellationToken).ConfigureAwait(false);

            return CustomerActivityOutcome.QueuedOffline;
        }
    }
}
