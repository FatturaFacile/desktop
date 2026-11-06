// <copyright file="CalculateVatReturnCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Taxation;

/// <summary>
/// Computes (or recomputes) the draft VAT return of a period for the current company.
/// </summary>
/// <param name="From">The first day.</param>
/// <param name="To">The last day.</param>
public sealed record CalculateVatReturnCommand(DateTime From, DateTime To) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
