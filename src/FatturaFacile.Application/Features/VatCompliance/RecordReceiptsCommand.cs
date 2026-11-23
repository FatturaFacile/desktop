// <copyright file="RecordReceiptsCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Records the takings of a day in the register of corrispettivi, split by rate or ventilated.
/// </summary>
/// <param name="Date">The day.</param>
/// <param name="GrossAmount">The gross takings.</param>
/// <param name="Allocations">The gross takings per rate; empty to ventilate.</param>
public sealed record RecordReceiptsCommand(DateTime Date, decimal GrossAmount, IReadOnlyList<ReceiptAllocation> Allocations) : IRequest<int>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
