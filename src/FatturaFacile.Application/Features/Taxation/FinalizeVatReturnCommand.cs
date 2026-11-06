// <copyright file="FinalizeVatReturnCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Taxation;

/// <summary>
/// Closes a VAT return and posts the offsetting of input VAT against the VAT due.
/// </summary>
/// <param name="Id">The VAT return identifier.</param>
public sealed record FinalizeVatReturnCommand(Guid Id) : IRequest<bool>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
