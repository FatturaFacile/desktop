// <copyright file="GetCustomerLifecycleQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.CustomerLifecycle;
using MediatR;

namespace FatturaFacile.Application.Features.CustomerLifecycle;

/// <summary>
/// Handler for <see cref="GetCustomerLifecycleQuery"/>.
/// </summary>
/// <param name="api">The web-api client.</param>
public sealed class GetCustomerLifecycleQueryHandler(ICustomerLifecycleApiClient api) : IRequestHandler<GetCustomerLifecycleQuery, CustomerLifecycleView?>
{
    /// <inheritdoc/>
    public Task<CustomerLifecycleView?> Handle(GetCustomerLifecycleQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return LoadAsync(request, cancellationToken);
    }

    private async Task<CustomerLifecycleView?> LoadAsync(GetCustomerLifecycleQuery request, CancellationToken cancellationToken)
    {
        var accountId = await api.FindAccountIdAsync(request.ContactId, cancellationToken).ConfigureAwait(false);
        if (accountId is null)
        {
            return null;
        }

        var summary = await api.GetSummaryAsync(accountId.Value, cancellationToken).ConfigureAwait(false);
        var timeline = await api.GetTimelineAsync(accountId.Value, request.Types, cancellationToken).ConfigureAwait(false);

        return new CustomerLifecycleView(summary, timeline, accountId.Value);
    }
}
