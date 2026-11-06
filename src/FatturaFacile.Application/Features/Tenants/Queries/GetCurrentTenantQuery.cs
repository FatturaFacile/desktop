// <copyright file="GetCurrentTenantQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Queries;

/// <summary>
/// Returns the master data of the active tenant.
/// </summary>
public sealed record GetCurrentTenantQuery : IRequest<TenantDto?>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CompanySettingsManage;
}
