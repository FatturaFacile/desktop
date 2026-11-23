// <copyright file="RecordThirdPartyGoodsCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Records goods held for third parties in the register of merci in conto terzi.
/// </summary>
/// <param name="Date">The date of the movement.</param>
/// <param name="DocumentNumber">The number of the delivery document.</param>
/// <param name="PartnerName">The owner of the goods.</param>
/// <param name="Description">The goods.</param>
/// <param name="Value">The value of the goods; negative when they are returned.</param>
public sealed record RecordThirdPartyGoodsCommand(DateTime Date, string DocumentNumber, string PartnerName, string Description, decimal Value) : IRequest<Guid>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.AccountingWrite;
}
