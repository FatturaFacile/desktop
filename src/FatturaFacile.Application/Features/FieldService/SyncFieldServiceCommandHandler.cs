// <copyright file="SyncFieldServiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="SyncFieldServiceCommand"/>.
/// </summary>
/// <param name="sync">The sync service.</param>
public sealed class SyncFieldServiceCommandHandler(IFieldServiceSyncService sync) : IRequestHandler<SyncFieldServiceCommand, FieldServiceCallResult>
{
    /// <inheritdoc/>
    public Task<FieldServiceCallResult> Handle(SyncFieldServiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return sync.SyncAsync(request.Scope, request.Key, request.From, request.To, cancellationToken);
    }
}
