// <copyright file="CreateQuoteCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Sales;
using FatturaFacile.Application.Strategies;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Commands;

/// <summary>
/// Handler for <see cref="CreateQuoteCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current tenant service.</param>
/// <param name="strategyFactory">The country strategy factory.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class CreateQuoteCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService,
    ICountryStrategyFactory strategyFactory,
    IMapper mapper) : IRequestHandler<CreateQuoteCommand, QuoteDto>
{
    /// <inheritdoc/>
    public Task<QuoteDto> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<QuoteDto> HandleInternalAsync(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        var tax = strategyFactory.GetTaxAndRoundingStrategy(currentTenantService.CountryCode);
        var contact = await unitOfWork.Repository<Contact>().GetByIdAsync(request.ContactId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Contact with ID {request.ContactId} was not found.");

        var totals = SalesDocumentCalculator.Calculate(request.Items.ToList(), tax);
        var existing = unitOfWork.Repository<Quote>().Query().Select(static q => q.QuoteNumber).ToList();

        Quote quote = new()
        {
            TenantId = currentTenantService.TenantId,
            ContactId = contact.Id,
            QuoteNumber = SalesDocumentNumbering.Next(existing, "QT", request.Date.Year),
            Date = request.Date,
            ValidUntil = request.ValidUntil ?? request.Date.AddDays(30),
            Currency = request.Currency ?? tax.DefaultCurrency,
            Subtotal = totals.Subtotal,
            DiscountAmount = totals.DiscountAmount,
            TotalNet = totals.TotalNet,
            TotalTax = totals.TotalTax,
            TotalGross = totals.TotalGross,
            PaymentTerms = request.PaymentTerms,
            ShippingTerms = request.ShippingTerms,
            Notes = request.Notes,
            Status = QuoteStatuses.Draft,
        };

        for (var i = 0; i < request.Items.Count; i++)
        {
            var line = request.Items[i];
            quote.Items.Add(new QuoteItem
            {
                TenantId = currentTenantService.TenantId,
                ProductId = line.ProductId,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                TaxRate = line.TaxRate,
                DiscountPercentage = line.DiscountPercentage,
                LineTotal = totals.LineTotals[i],
                Uom = line.Uom,
                Notes = line.Notes,
            });
        }

        await unitOfWork.Repository<Quote>().AddAsync(quote, cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        quote.Contact = contact;
        return mapper.Map<QuoteDto>(quote);
    }
}
