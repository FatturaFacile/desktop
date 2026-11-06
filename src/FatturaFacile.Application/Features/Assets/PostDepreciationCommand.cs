// <copyright file="PostDepreciationCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Assets;

/// <summary>
/// Posts the yearly depreciation of all fixed assets not yet depreciated in the year.
/// </summary>
/// <param name="Year">The fiscal year.</param>
public sealed record PostDepreciationCommand(int Year) : IRequest<int>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
