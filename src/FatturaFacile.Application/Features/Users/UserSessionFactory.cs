// <copyright file="UserSessionFactory.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Security;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Features.Users;

/// <summary>
/// Builds the authenticated session of a user and activates it, selecting an accessible tenant.
/// </summary>
public static class UserSessionFactory
{
    /// <summary>
    /// Loads the roles of the user and builds the session.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authenticated user.</returns>
    public static Task<AuthenticatedUser> CreateAsync(AppUser user, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        return CreateInternalAsync(user, unitOfWork, cancellationToken);
    }

    /// <summary>
    /// Signs the session in and, when the active tenant is not accessible, switches to the first accessible tenant.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="currentTenantService">The current tenant service.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the activation.</returns>
    public static Task ActivateAsync(
        AuthenticatedUser session,
        ICurrentUserService currentUserService,
        ICurrentTenantService currentTenantService,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(currentUserService);
        ArgumentNullException.ThrowIfNull(currentTenantService);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        currentUserService.SignIn(session);

        return session.TenantRoles.ContainsKey(currentTenantService.TenantId) || session.TenantRoles.Count == 0
            ? Task.CompletedTask
            : SwitchToFirstTenantAsync(session, currentTenantService, unitOfWork, cancellationToken);
    }

    private static async Task<AuthenticatedUser> CreateInternalAsync(AppUser user, IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        var tenants = await unitOfWork.Repository<Tenant>()
            .FindAsync(static _ => true, cancellationToken)
            .ConfigureAwait(false);

        Dictionary<Guid, UserRole> roles = [];

        if (user.IsSystemAdministrator)
        {
            foreach (var tenant in tenants)
            {
                roles[tenant.Id] = UserRole.Administrator;
            }
        }
        else
        {
            var userId = user.Id;
            var grants = await unitOfWork.Repository<UserTenantRole>()
                .FindAsync(r => r.UserId == userId, cancellationToken)
                .ConfigureAwait(false);

            foreach (var grant in grants)
            {
                if (Enum.TryParse<UserRole>(grant.Role, ignoreCase: false, out var role) &&
                    tenants.Any(t => t.Id == grant.TenantId))
                {
                    roles[grant.TenantId] = role;
                }
            }
        }

        return new AuthenticatedUser(user.Id, user.UserName, user.DisplayName, user.IsSystemAdministrator, roles);
    }

    private static async Task SwitchToFirstTenantAsync(
        AuthenticatedUser session,
        ICurrentTenantService currentTenantService,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var tenant = await unitOfWork.Repository<Tenant>()
            .GetByIdAsync(session.TenantRoles.Keys.First(), cancellationToken)
            .ConfigureAwait(false);

        if (tenant is not null)
        {
            currentTenantService.SetCurrentTenant(tenant.Id, tenant.Name, tenant.Country, tenant.DefaultCurrency);
        }
    }
}
