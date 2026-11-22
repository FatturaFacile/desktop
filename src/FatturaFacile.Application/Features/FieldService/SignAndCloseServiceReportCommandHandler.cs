// <copyright file="SignAndCloseServiceReportCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.FieldService;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Handles <see cref="SignAndCloseServiceReportCommand"/>.
/// </summary>
/// <param name="offline">The offline-first write service.</param>
public sealed class SignAndCloseServiceReportCommandHandler(IFieldServiceOfflineService offline) : IRequestHandler<SignAndCloseServiceReportCommand, FieldServiceQueueResult>
{
    /// <inheritdoc/>
    public Task<FieldServiceQueueResult> Handle(SignAndCloseServiceReportCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return offline.QueueReportAsync(request.Report, cancellationToken);
    }
}
