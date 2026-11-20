// <copyright file="ChangeQuoteStatusCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using AutoMapper.QueryableExtensions;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Sales;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Commands;

/// <summary>
/// Handler for <see cref="ChangeQuoteStatusCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="mapper">The AutoMapper instance.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class ChangeQuoteStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, TimeProvider timeProvider)
    : IRequestHandler<ChangeQuoteStatusCommand, QuoteDto>
{
    /// <inheritdoc/>
    public Task<QuoteDto> Handle(ChangeQuoteStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<QuoteDto> HandleInternalAsync(ChangeQuoteStatusCommand request, CancellationToken cancellationToken)
    {
        var quote = await unitOfWork.Repository<Quote>().GetByIdAsync(request.QuoteId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Quote with ID {request.QuoteId} was not found.");

        SalesDocumentWorkflow.AssertQuoteTransition(quote.Status, request.TargetStatus);

        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (string.Equals(request.TargetStatus, QuoteStatuses.Accepted, StringComparison.Ordinal)
            && quote.ValidUntil is { } validUntil && validUntil.Date < now.Date)
        {
            throw new InvalidOperationException("The quote has expired and can no longer be accepted.");
        }

        if (string.Equals(request.TargetStatus, QuoteStatuses.Approved, StringComparison.Ordinal))
        {
            quote.ApprovedAt = now;
        }

        quote.Status = request.TargetStatus;
        quote.UpdatedAt = now;
        unitOfWork.Repository<Quote>().Update(quote);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return unitOfWork.Repository<Quote>().Query()
            .Where(q => q.Id == quote.Id)
            .ProjectTo<QuoteDto>(mapper.ConfigurationProvider)
            .Single();
    }
}
