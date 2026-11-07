// <copyright file="SignInWindowsUserCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Handler for <see cref="SignInWindowsUserCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SignInWindowsUserCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentUserService">The current user service.</param>
/// <param name="currentTenantService">The current tenant service.</param>
public sealed class SignInWindowsUserCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ICurrentTenantService currentTenantService) : IRequestHandler<SignInWindowsUserCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(SignInWindowsUserCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(
        SignInWindowsUserCommand request,
        CancellationToken cancellationToken)
    {
        var accountName = request.WindowsAccountName.Trim().ToUpperInvariant();

        if (accountName.Length == 0)
        {
            return false;
        }

        var users = unitOfWork.Repository<AppUser>();
        var existing = await users.FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);

        AppUser? user;

        if (existing.Count == 0)
        {
            user = new AppUser
            {
                UserName = accountName,
                DisplayName =
                    string.IsNullOrWhiteSpace(request.DisplayName)
                        ? accountName
                        : request.DisplayName.Trim(),
                WindowsAccountName = accountName,
                IsSystemAdministrator = true,
                LastSignInUtc = DateTime.UtcNow,
            };

            await users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            user = existing.FirstOrDefault(u => u.IsActive && string.Equals(
                u.WindowsAccountName,
                accountName,
                StringComparison.Ordinal));

            if (user is null)
            {
                return false;
            }

            user.LastSignInUtc = DateTime.UtcNow;
            users.Update(user);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var session = await UserSessionFactory.CreateAsync(user, unitOfWork, cancellationToken)
           .ConfigureAwait(false);
        await UserSessionFactory.ActivateAsync(
            session,
            currentUserService,
            currentTenantService,
            unitOfWork,
            cancellationToken).ConfigureAwait(false);

        return true;
    }
}
