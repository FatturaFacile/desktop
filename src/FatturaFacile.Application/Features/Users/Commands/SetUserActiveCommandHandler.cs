// <copyright file="SetUserActiveCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Handler for <see cref="SetUserActiveCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SetUserActiveCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class SetUserActiveCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<SetUserActiveCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return !request.IsActive && request.UserId == currentUserService.User?.UserId
            ? throw new InvalidOperationException("Non è possibile disattivare il proprio utente.")
            : HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        var users = unitOfWork.Repository<AppUser>();
        var user = await users.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            return false;
        }

        user.IsActive = request.IsActive;
        users.Update(user);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
