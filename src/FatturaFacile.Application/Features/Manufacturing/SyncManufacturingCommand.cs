// <copyright file="SyncManufacturingCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.Manufacturing;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Manufacturing;

/// <summary>
/// Command that pulls the manufacturing master data and OEE summaries from the web-api into the local cache.
/// </summary>
public sealed record SyncManufacturingCommand : IRequest<ManufacturingSyncResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CommercialRead;
}
