// <copyright file="AssignTenantRoleCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using FatturaFacile.Domain.Enums;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Handler for <see cref="AssignTenantRoleCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AssignTenantRoleCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class AssignTenantRoleCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<AssignTenantRoleCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(AssignTenantRoleCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return !currentUserService.HasPermission(request.TenantId, Permission.UsersManage)
            ? throw new UnauthorizedAccessException("Non è consentito gestire gli utenti dell'azienda indicata.")
            : HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(AssignTenantRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await unitOfWork.Repository<AppUser>().GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return false;
        }

        var roles = unitOfWork.Repository<UserTenantRole>();
        var existing = await roles
            .FindAsync(r => r.UserId == request.UserId && r.TenantId == request.TenantId, cancellationToken)
            .ConfigureAwait(false);

        if (existing.Count > 0)
        {
            existing[0].Role = request.Role.ToString();
            roles.Update(existing[0]);
        }
        else
        {
            UserTenantRole grant = new()
            {
                UserId = request.UserId,
                TenantId = request.TenantId,
                Role = request.Role.ToString(),
            };

            await roles.AddAsync(grant, cancellationToken).ConfigureAwait(false);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
