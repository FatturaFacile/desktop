// <copyright file="CreateFirstAdministratorCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Handler for <see cref="CreateFirstAdministratorCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateFirstAdministratorCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="passwordHasher">The password hasher.</param>
/// <param name="currentUserService">The current user service.</param>
/// <param name="currentTenantService">The current tenant service.</param>
public sealed class CreateFirstAdministratorCommandHandler(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    ICurrentTenantService currentTenantService)
    : IRequestHandler<CreateFirstAdministratorCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(CreateFirstAdministratorCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(CreateFirstAdministratorCommand request, CancellationToken cancellationToken)
    {
        var users = unitOfWork.Repository<AppUser>();
        var existing = await users.FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);

        if (existing.Count > 0)
        {
            throw new InvalidOperationException("L'amministratore iniziale è già stato creato.");
        }

        var userName = request.UserName.Trim();
        AppUser user = new()
        {
            UserName = userName,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? userName : request.DisplayName.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            IsSystemAdministrator = true,
            LastSignInUtc = DateTime.UtcNow,
        };

        await users.AddAsync(user, cancellationToken).ConfigureAwait(false);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var session = await UserSessionFactory.CreateAsync(user, unitOfWork, cancellationToken).ConfigureAwait(false);
        await UserSessionFactory.ActivateAsync(session, currentUserService, currentTenantService, unitOfWork, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}
