// <copyright file="GetManufacturingWorkbenchQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Manufacturing.Features;

/// <summary>
/// Loads the offline workbench data (never touches the network).
/// </summary>
public sealed record GetManufacturingWorkbenchQuery : IRequest<ManufacturingWorkbench>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.ManufacturingRead;
}
