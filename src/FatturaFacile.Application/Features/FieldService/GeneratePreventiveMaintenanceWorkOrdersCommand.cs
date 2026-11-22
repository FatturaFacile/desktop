// <copyright file="GeneratePreventiveMaintenanceWorkOrdersCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Runs the preventive maintenance engine of the server now: it creates the work orders of the calendar and meter cycles that are due, once per cycle. Needs a connection.
/// </summary>
public sealed record GeneratePreventiveMaintenanceWorkOrdersCommand : IRequest<FieldServicePreventiveResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
