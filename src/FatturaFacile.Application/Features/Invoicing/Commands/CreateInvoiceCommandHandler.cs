// <copyright file="CreateInvoiceCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Strategies;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Invoicing.Commands;

/// <summary>
/// Handler for <see cref="CreateInvoiceCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateInvoiceCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current tenant service.</param>
/// <param name="strategyFactory">The country strategy factory.</param>
/// <param name="mapper">The AutoMapper instance.</param>
public sealed class CreateInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentTenantService currentTenantService,
    ICountryStrategyFactory strategyFactory,
    IMapper mapper) : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    /// <inheritdoc/>
    public Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<InvoiceDto> HandleInternalAsync(
        CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        var country = currentTenantService.CountryCode;
        var taxStrategy = strategyFactory.GetTaxAndRoundingStrategy(country);
        var invoicingStrategy = strategyFactory.GetInvoicingStrategy(country);

        var contact =
            await unitOfWork.Repository<Contact>().GetByIdAsync(request.ContactId, cancellationToken)
               .ConfigureAwait(false) ??
            throw new KeyNotFoundException($"Contact with ID {request.ContactId} was not found.");

        var tenant = await unitOfWork.Repository<Tenant>()
           .GetByIdAsync(currentTenantService.TenantId, cancellationToken).ConfigureAwait(false);

        tenant ??= new Tenant
        {
            Id = currentTenantService.TenantId,
            Name = currentTenantService.TenantName,
            LegalName = currentTenantService.TenantName,
            Country = country,
            DefaultCurrency = currentTenantService.DefaultCurrency,
        };

        Invoice invoice = new()
        {
            TenantId = currentTenantService.TenantId,
            ContactId = contact.Id,
            InvoiceNumber = request.InvoiceNumber,
            Date = request.Date,
            DueDate = request.DueDate ?? request.Date.AddDays(contact.PaymentTermsDays),
            Currency = request.Currency ?? taxStrategy.DefaultCurrency,
            Jurisdiction = country,
            SplitPayment = request.SplitPayment,
            Notes = request.Notes,
            Status = "draft",
            PaymentStatus = "unpaid",
        };

        var subtotal = 0m;
        var totalTax = 0m;

        foreach (var itemDto in request.Items)
        {
            var discountFactor = 1m - (itemDto.DiscountPercentage / 100m);

            var lineTotal = Math.Round(
                itemDto.Quantity * itemDto.UnitPrice * discountFactor,
                2,
                MidpointRounding.AwayFromZero);

            var lineTax = taxStrategy.CalculateTax(lineTotal, itemDto.TaxRate);

            subtotal += lineTotal;
            totalTax += lineTax;

            InvoiceItem item = new()
            {
                TenantId = currentTenantService.TenantId,
                ProductId = itemDto.ProductId,
                Description = itemDto.Description,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TaxRate = itemDto.TaxRate,
                DiscountPercentage = itemDto.DiscountPercentage,
                LineTotal = lineTotal,
                Uom = itemDto.Uom,
                Notes = itemDto.Notes,
            };

            invoice.Items.Add(item);
        }

        invoice.Subtotal = subtotal;
        invoice.TotalNet = subtotal;
        invoice.TotalTax = totalTax;

        var unroundedGross = subtotal + totalTax;
        var roundedGross = taxStrategy.RoundFinalAmount(unroundedGross);

        invoice.RoundingAmount = taxStrategy.CalculateRoundingAdjustment(unroundedGross, roundedGross);

        invoice.TotalGross = roundedGross;
        invoice.ReceivableAmount = roundedGross;

        await invoicingStrategy
           .ProcessInvoiceComplianceAsync(invoice, tenant, contact, cancellationToken)
           .ConfigureAwait(false);

        await unitOfWork.Repository<Invoice>().AddAsync(invoice, cancellationToken)
           .ConfigureAwait(false);

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        invoice.Contact = contact;

        return mapper.Map<InvoiceDto>(invoice);
    }
}
