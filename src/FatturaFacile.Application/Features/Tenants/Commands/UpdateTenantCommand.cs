// <copyright file="UpdateTenantCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Tenants.Commands;

/// <summary>
/// Updates the master data of the active tenant.
/// </summary>
public sealed class UpdateTenantCommand : IRequest<TenantDto>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.CompanySettingsManage;

    /// <summary>
    /// Gets or sets the tenant data (the identifier must be the active tenant).
    /// </summary>
    public TenantDto Tenant { get; set; } = new();
}
