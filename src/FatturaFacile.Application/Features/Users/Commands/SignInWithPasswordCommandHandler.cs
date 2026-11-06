// <copyright file="SignInWithPasswordCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Handler for <see cref="SignInWithPasswordCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SignInWithPasswordCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="passwordHasher">The password hasher.</param>
/// <param name="currentUserService">The current user service.</param>
/// <param name="currentTenantService">The current tenant service.</param>
public sealed class SignInWithPasswordCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<SignInWithPasswordCommand, bool>
{
    /// <summary>
    /// The number of consecutive failures that triggers a lockout.
    /// </summary>
    public const int MaxFailedAttempts = 5;

    private static readonly TimeSpan _lockoutDuration = TimeSpan.FromMinutes(15);

    /// <inheritdoc/>
    public Task<bool> Handle(SignInWithPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(SignInWithPasswordCommand request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        var users = unitOfWork.Repository<AppUser>();
        var all = await users.FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var user = all.FirstOrDefault(u => u.IsActive && string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));

        if (user?.PasswordHash is null || user.LockoutEndUtc > DateTime.UtcNow)
        {
            return false;
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedSignInCount++;

            if (user.FailedSignInCount >= MaxFailedAttempts)
            {
                user.LockoutEndUtc = DateTime.UtcNow.Add(_lockoutDuration);
                user.FailedSignInCount = 0;
            }

            users.Update(user);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return false;
        }

        user.FailedSignInCount = 0;
        user.LockoutEndUtc = null;
        user.LastSignInUtc = DateTime.UtcNow;
        users.Update(user);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var session = await UserSessionFactory.CreateAsync(user, unitOfWork, cancellationToken).ConfigureAwait(false);
        await UserSessionFactory.ActivateAsync(session, currentUserService, currentTenantService, unitOfWork, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}
