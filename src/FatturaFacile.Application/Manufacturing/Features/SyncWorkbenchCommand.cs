// <copyright file="SyncWorkbenchCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Manufacturing.Sync;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Pushes the manufacturing outbox and pulls the delta from the web-api.
/// </summary>
public sealed record SyncWorkbenchCommand : IRequest<WorkbenchSyncResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.ManufacturingRead;
}
