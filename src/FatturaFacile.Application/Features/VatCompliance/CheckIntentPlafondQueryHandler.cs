// <copyright file="CheckIntentPlafondQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="CheckIntentPlafondQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class CheckIntentPlafondQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<CheckIntentPlafondQuery, PlafondCheckResult>
{
    /// <inheritdoc/>
    public Task<PlafondCheckResult> Handle(CheckIntentPlafondQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<PlafondCheckResult> HandleInternalAsync(CheckIntentPlafondQuery request, CancellationToken cancellationToken)
    {
        var (letters, usage) = await IntentLetterLookup.LoadAsync(unitOfWork, cancellationToken).ConfigureAwait(false);

        return IntentLetterPlafond.Evaluate(letters, usage, request.ContactId, request.Date, request.Amount);
    }
}
