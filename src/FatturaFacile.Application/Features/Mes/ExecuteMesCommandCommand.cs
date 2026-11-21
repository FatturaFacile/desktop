// <copyright file="ExecuteMesCommandCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Mes;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Mes;

/// <summary>
/// Command that performs an operator action: it is validated against the state machine, saved in the outbox and sent.
/// </summary>
/// <param name="Command">The action.</param>
/// <param name="Row">The phase it acts on, as the operator sees it.</param>
public sealed record ExecuteMesCommandCommand(MesCommand Command, MesPhaseRow Row) : IRequest<MesExecuteResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.TimeClock;
}
