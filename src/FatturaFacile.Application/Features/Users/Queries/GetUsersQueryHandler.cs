// <copyright file="GetUsersQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Queries;

/// <summary>
/// Handler for <see cref="GetUsersQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetUsersQueryHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class GetUsersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await unitOfWork.Repository<AppUser>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var grants = await unitOfWork.Repository<UserTenantRole>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);

        var result = users
            .OrderBy(static u => u.UserName, StringComparer.OrdinalIgnoreCase)
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                DisplayName = u.DisplayName,
                WindowsAccountName = u.WindowsAccountName,
                IsActive = u.IsActive,
                IsSystemAdministrator = u.IsSystemAdministrator,
                TenantRoles = grants
                    .Where(g => g.UserId == u.Id && currentUserService.HasPermission(g.TenantId, Permission.UsersManage))
                    .ToDictionary(static g => g.TenantId, static g => g.Role),
            });

        return [.. result];
    }
}
