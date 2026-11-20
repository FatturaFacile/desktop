// <copyright file="CreateInvoiceFromOrderCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Application.Features.Invoicing.Commands;
using FatturaFacile.Application.Sales;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Sales.Commands;

/// <summary>
/// Handler for <see cref="CreateInvoiceFromOrderCommand"/>: reuses the invoice creation (tax, rounding, compliance).
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="sender">The MediatR sender.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class CreateInvoiceFromOrderCommandHandler(IUnitOfWork unitOfWork, ISender sender, TimeProvider timeProvider)
    : IRequestHandler<CreateInvoiceFromOrderCommand, InvoiceDto>
{
    private static readonly string[] Billable =
    [
        SalesOrderStatuses.Confirmed,
        SalesOrderStatuses.Approved,
        SalesOrderStatuses.InProduction,
        SalesOrderStatuses.InFulfillment,
        SalesOrderStatuses.Shipped,
        SalesOrderStatuses.Completed,
    ];

    /// <inheritdoc/>
    public Task<InvoiceDto> Handle(CreateInvoiceFromOrderCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private static InvoiceItemDto ToInvoiceLine(SalesOrderItem item) => new()
    {
        ProductId = item.ProductId,
        Description = item.Description,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        TaxRate = item.TaxRate,
        DiscountPercentage = item.DiscountPercentage,
        Uom = item.Uom,
        Notes = item.Notes,
    };

    private async Task<InvoiceDto> HandleInternalAsync(CreateInvoiceFromOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Repository<SalesOrder>().GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Order with ID {request.OrderId} was not found.");

        if (!Billable.Contains(order.Status, StringComparer.Ordinal))
        {
            throw new InvalidOperationException("Only a confirmed order can be invoiced.");
        }

        if (order.InvoiceId is not null)
        {
            throw new InvalidOperationException("The order has already been invoiced.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var items = await unitOfWork.Repository<SalesOrderItem>().FindAsync(i => i.SalesOrderId == order.Id, cancellationToken).ConfigureAwait(false);
        var existing = unitOfWork.Repository<Invoice>().Query().Select(static i => i.InvoiceNumber).ToList();

        CreateInvoiceCommand command = new()
        {
            ContactId = order.ContactId,
            InvoiceNumber = SalesDocumentNumbering.Next(existing, "INV", now.Year),
            Date = now.Date,
            Currency = order.Currency,
            Notes = order.Notes,
            Items = [.. items.Select(ToInvoiceLine)],
        };

        var invoice = await sender.Send(command, cancellationToken).ConfigureAwait(false);

        order.InvoiceId = invoice.Id;
        order.UpdatedAt = now;
        unitOfWork.Repository<SalesOrder>().Update(order);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return invoice;
    }
}
