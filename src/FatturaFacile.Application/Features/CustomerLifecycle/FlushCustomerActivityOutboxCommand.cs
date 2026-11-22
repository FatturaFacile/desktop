// <copyright file="FlushCustomerActivityOutboxCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.CustomerLifecycle;

/// <summary>
/// Command that replays the customer quick actions saved offline, oldest first. Answers the number of activities sent.
/// </summary>
public sealed record FlushCustomerActivityOutboxCommand : IRequest<int>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
