// <copyright file="FlushMesOutboxCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Command that sends the pending operator commands to the server, in the order the operator acted.
/// </summary>
public sealed record FlushMesOutboxCommand : IRequest<MesFlushResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
