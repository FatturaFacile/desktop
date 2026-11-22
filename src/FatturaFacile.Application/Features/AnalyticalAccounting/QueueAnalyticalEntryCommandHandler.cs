// <copyright file="QueueAnalyticalEntryCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AnalyticalAccounting;
using MediatR;

namespace FatturaFacile.Application.Features.AnalyticalAccounting;

/// <summary>
/// Handles <see cref="QueueAnalyticalEntryCommand"/>.
/// </summary>
/// <param name="queue">The offline queue.</param>
public sealed class QueueAnalyticalEntryCommandHandler(IAnalyticalOperationQueue queue) : IRequestHandler<QueueAnalyticalEntryCommand, QueueAnalyticalEntryResult>
{
    /// <inheritdoc/>
    public Task<QueueAnalyticalEntryResult> Handle(QueueAnalyticalEntryCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = request.Draft.Validate();

        return errors.Count > 0
            ? Task.FromResult(new QueueAnalyticalEntryResult(OperationId: null, errors))
            : EnqueueAsync(request.Draft, cancellationToken);
    }

    private async Task<QueueAnalyticalEntryResult> EnqueueAsync(AnalyticalEntryDraft draft, CancellationToken cancellationToken)
    {
        var id = await queue.EnqueueAsync(draft.ToApiJson(), cancellationToken).ConfigureAwait(false);

        return new QueueAnalyticalEntryResult(id, []);
    }
}
