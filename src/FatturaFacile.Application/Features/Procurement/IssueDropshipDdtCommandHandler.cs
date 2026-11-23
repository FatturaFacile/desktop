// <copyright file="IssueDropshipDdtCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="IssueDropshipDdtCommand"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class IssueDropshipDdtCommandHandler(IProcurementService service) : IRequestHandler<IssueDropshipDdtCommand, ProcurementActionResult>
{
    /// <inheritdoc/>
    public Task<ProcurementActionResult> Handle(IssueDropshipDdtCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.IssueDdtAsync(request.Request, cancellationToken);
    }
}
