// <copyright file="GetUsersQuery.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Security;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Queries;

/// <summary>
/// Lists the users with the roles held in the tenants the caller manages.
/// </summary>
public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>, IAuthorizedRequest
{
    /// <inheritdoc/>
    public Permission RequiredPermission => Permission.UsersManage;
}
