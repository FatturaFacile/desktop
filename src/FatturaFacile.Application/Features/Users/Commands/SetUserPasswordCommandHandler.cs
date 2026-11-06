// <copyright file="SetUserPasswordCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Handler for <see cref="SetUserPasswordCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SetUserPasswordCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="passwordHasher">The password hasher.</param>
public sealed class SetUserPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    : IRequestHandler<SetUserPasswordCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(SetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(SetUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var users = unitOfWork.Repository<AppUser>();
        var user = await users.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return false;
        }

        user.PasswordHash = passwordHasher.Hash(request.NewPassword);
        user.FailedSignInCount = 0;
        user.LockoutEndUtc = null;
        users.Update(user);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
