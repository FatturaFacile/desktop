// <copyright file="ConvertQuoteToOrderCommandHandler.cs" company="Fattura Facile">
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
/// Handler for <see cref="ConvertQuoteToOrderCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="mapper">The AutoMapper instance.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class ConvertQuoteToOrderCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, TimeProvider timeProvider)
    : IRequestHandler<ConvertQuoteToOrderCommand, SalesOrderDto>
{
    /// <inheritdoc/>
    public Task<SalesOrderDto> Handle(ConvertQuoteToOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<SalesOrderDto> HandleInternalAsync(ConvertQuoteToOrderCommand request, CancellationToken cancellationToken)
    {
        var quote = await unitOfWork.Repository<Quote>().GetByIdAsync(request.QuoteId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Quote with ID {request.QuoteId} was not found.");

        if (!string.Equals(quote.Status, QuoteStatuses.Accepted, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Only an accepted quote can be converted into an order.");
        }

        if (quote.ConvertedToOrderId is not null)
        {
            throw new InvalidOperationException("The quote has already been converted into an order.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var today = now.Date;
        var items = await unitOfWork.Repository<QuoteItem>().FindAsync(i => i.QuoteId == quote.Id, cancellationToken).ConfigureAwait(false);
        var existing = unitOfWork.Repository<SalesOrder>().Query().Select(static o => o.OrderNumber).ToList();

        SalesOrder order = new()
        {
            TenantId = quote.TenantId,
            ContactId = quote.ContactId,
            QuoteId = quote.Id,
            OrderNumber = SalesDocumentNumbering.Next(existing, "SO", today.Year),
            Date = today,
            DueDate = today.AddDays(30),
            Currency = quote.Currency,
            Subtotal = quote.Subtotal,
            DiscountAmount = quote.DiscountAmount,
            TotalNet = quote.TotalNet,
            TotalTax = quote.TotalTax,
            TotalGross = quote.TotalGross,
            PaymentTerms = quote.PaymentTerms,
            ShippingMethod = quote.ShippingTerms,
            Notes = quote.Notes,
            Status = SalesOrderStatuses.Draft,
        };

        foreach (var item in items)
        {
            order.Items.Add(new SalesOrderItem
            {
                TenantId = quote.TenantId,
                ProductId = item.ProductId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                DiscountPercentage = item.DiscountPercentage,
                LineTotal = item.LineTotal,
                Uom = item.Uom,
                Notes = item.Notes,
            });
        }

        await unitOfWork.Repository<SalesOrder>().AddAsync(order, cancellationToken).ConfigureAwait(false);
        quote.ConvertedToOrderId = order.Id;
        quote.UpdatedAt = now;
        unitOfWork.Repository<Quote>().Update(quote);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return unitOfWork.Repository<SalesOrder>().Query()
            .Where(o => o.Id == order.Id)
            .ProjectTo<SalesOrderDto>(mapper.ConfigurationProvider)
            .Single();
    }
}
