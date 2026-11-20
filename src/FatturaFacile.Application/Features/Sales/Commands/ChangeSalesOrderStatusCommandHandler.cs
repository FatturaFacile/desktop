// <copyright file="ChangeSalesOrderStatusCommandHandler.cs" company="Fattura Facile">
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
/// Handler for <see cref="ChangeSalesOrderStatusCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="mapper">The AutoMapper instance.</param>
/// <param name="timeProvider">The clock.</param>
public sealed class ChangeSalesOrderStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, TimeProvider timeProvider)
    : IRequestHandler<ChangeSalesOrderStatusCommand, SalesOrderDto>
{
    /// <inheritdoc/>
    public Task<SalesOrderDto> Handle(ChangeSalesOrderStatusCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<SalesOrderDto> HandleInternalAsync(ChangeSalesOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await unitOfWork.Repository<SalesOrder>().GetByIdAsync(request.OrderId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Order with ID {request.OrderId} was not found.");

        SalesDocumentWorkflow.AssertOrderTransition(order.Status, request.TargetStatus);

        order.Status = request.TargetStatus;
        order.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        unitOfWork.Repository<SalesOrder>().Update(order);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return unitOfWork.Repository<SalesOrder>().Query()
            .Where(o => o.Id == order.Id)
            .ProjectTo<SalesOrderDto>(mapper.ConfigurationProvider)
            .Single();
    }
}
