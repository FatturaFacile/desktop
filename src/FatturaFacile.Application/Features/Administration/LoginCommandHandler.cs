// <copyright file="LoginCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Handles authentication credentials, verifies password hashes, establishes tenant and user execution contexts, and updates login audit timestamps.
/// </summary>
/// <param name="userRepository">The repository used to locate and update operator user accounts.</param>
/// <param name="tenantRepository">The repository used to validate target tenant organization existence.</param>
/// <param name="tenantProvider">The tenant provider used to configure the active tenant execution boundary upon successful authentication.</param>
/// <param name="currentUserService">The current user service used to bind authenticated identity context.</param>
/// <param name="passwordHasher">The password hashing component used to verify cryptographic credentials.</param>
/// <param name="mapper">The mapper used to transform domain entities to user transfer objects.</param>
public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    ITenantProvider tenantProvider,
    ICurrentUserService currentUserService,
    IPasswordHasher passwordHasher,
    IMapper mapper) : IRequestHandler<LoginCommand, UserDto>
{
    private readonly IUserRepository userRepository = userRepository;

    private readonly ITenantRepository tenantRepository = tenantRepository;

    private readonly ITenantProvider tenantProvider = tenantProvider;

    private readonly ICurrentUserService currentUserService = currentUserService;

    private readonly IPasswordHasher passwordHasher = passwordHasher;

    private readonly IMapper mapper = mapper;

    /// <summary>
    /// Authenticates user credentials, binds the tenant and user session context, and updates the user's last login audit date.
    /// </summary>
    /// <param name="request">The login command containing credentials and target tenant.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the login workflow returning the authenticated <see cref="UserDto"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the target tenant does not exist, credentials are invalid, or the user account is locked/deactivated.</exception>
    public async Task<UserDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tenant = await tenantRepository.GetByIdAsync(
                         request.TenantId,
                         cancellationToken)
                     ?? throw new UnauthorizedAccessException("Selected tenant does not exist or is inactive.");

        var user = await userRepository.GetByUsernameAsync(
                       request.Username,
                       cancellationToken)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is locked or deactivated.");
        }

        var isValid = passwordHasher.VerifyHash(
            request.Password,
            user.PasswordHash,
            user.PasswordSalt);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        user.RecordLogin();
        await userRepository.UpdateAsync(
            user,
            cancellationToken);

        tenantProvider.SetTenant(
            tenant.Id,
            tenant.Code);
        currentUserService.SetUser(
            user.Id,
            user.Username,
            user.FullName,
            user.Role.ToString());

        return mapper.Map<UserDto>(user);
    }
}
