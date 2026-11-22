// <copyright file="SaveMaintenanceContractCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.FieldService;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.FieldService;

/// <summary>
/// Creates or edits a maintenance contract (coverage type, preventive frequency, included visits, call-out fee). Needs a connection.
/// </summary>
/// <param name="Contract">The contract fields; a null identifier creates a new contract.</param>
public sealed record SaveMaintenanceContractCommand(FieldServiceContractInput Contract) : IRequest<FieldServiceCallResult>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.MasterDataWrite;
}
