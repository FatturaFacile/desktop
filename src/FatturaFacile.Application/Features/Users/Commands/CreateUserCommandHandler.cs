// <copyright file="CreateUserCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Handler for <see cref="CreateUserCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateUserCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="passwordHasher">The password hasher.</param>
/// <param name="currentUserService">The current user service.</param>
/// <param name="currentTenantService">The current tenant service.</param>
public sealed class CreateUserCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<CreateUserCommand, Guid>
{
    /// <inheritdoc/>
    public Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.IsSystemAdministrator && currentUserService.User?.IsSystemAdministrator != true
            ? throw new UnauthorizedAccessException("Solo un amministratore di sistema può creare altri amministratori di sistema.")
            : HandleInternalAsync(request, cancellationToken);
    }

    private async Task<Guid> HandleInternalAsync(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        var users = unitOfWork.Repository<AppUser>();
        var all = await users.FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);

        if (all.Any(u => string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Esiste già un utente '{userName}'.");
        }

        AppUser user = new()
        {
            UserName = userName,
            DisplayName = request.DisplayName.Trim(),
            WindowsAccountName = string.IsNullOrWhiteSpace(request.WindowsAccountName) ? null : request.WindowsAccountName.Trim().ToUpperInvariant(),
            PasswordHash = request.Password is null ? null : passwordHasher.Hash(request.Password),
            IsSystemAdministrator = request.IsSystemAdministrator,
        };

        await users.AddAsync(user, cancellationToken).ConfigureAwait(false);

        if (request.InitialRole is { } role)
        {
            UserTenantRole grant = new()
            {
                UserId = user.Id,
                TenantId = currentTenantService.TenantId,
                Role = role.ToString(),
            };

            await unitOfWork.Repository<UserTenantRole>().AddAsync(grant, cancellationToken).ConfigureAwait(false);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return user.Id;
    }
}
