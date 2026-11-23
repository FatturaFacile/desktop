// <copyright file="GetOrderRemindersQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Procurement;
using MediatR;

namespace FatturaFacile.Application.Features.Procurement;

/// <summary>
/// Handler for <see cref="GetOrderRemindersQuery"/>.
/// </summary>
/// <param name="service">The procurement service.</param>
public sealed class GetOrderRemindersQueryHandler(IProcurementService service) : IRequestHandler<GetOrderRemindersQuery, ProcurementLoad<IReadOnlyList<ReminderView>>?>
{
    /// <inheritdoc/>
    public Task<ProcurementLoad<IReadOnlyList<ReminderView>>?> Handle(GetOrderRemindersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return service.LoadRemindersAsync(request.OrderId, cancellationToken);
    }
}
