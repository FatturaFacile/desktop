// <copyright file="IdentityCommandHandlers.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Shared.Contracts;
using FatturaFacile.Application.Shared.Results;
using FatturaFacile.Domain.Shared.Errors;
using FatturaFacile.Identity.Application.Commands;
using FatturaFacile.Identity.Application.DTOs;
using FatturaFacile.Identity.Application.Security;
using FatturaFacile.Identity.Domain.Aggregates;
using FatturaFacile.Identity.Domain.Entities;
using FatturaFacile.Identity.Domain.Repositories;

namespace FatturaFacile.Identity.Application.Handlers;

/// <summary>
/// CQRS handler for user authentication, token refresh, and tenant user onboarding.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IdentityCommandHandlers"/> class.
/// </remarks>
/// <param name="tenantRepository">The tenant repository.</param>
/// <param name="userRepository">The user repository.</param>
/// <param name="refreshTokenRepository">The refresh token repository.</param>
/// <param name="auditLogRepository">The security audit log repository.</param>
/// <param name="passwordHasher">The Argon2id password hasher service.</param>
/// <param name="dataProtector">The data protector service for AES-GCM and blind-indexing.</param>
/// <param name="tokenService">The JWT and token generation service.</param>
/// <param name="jwtOptions">The optional JWT configuration options.</param>
public sealed class IdentityCommandHandlers(
    ITenantRepository tenantRepository,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ISecurityAuditLogRepository auditLogRepository,
    IPasswordHasher passwordHasher,
    IDataProtectorService dataProtector,
    ITokenService tokenService,
    JwtOptions? jwtOptions = null) :
    ICommandHandler<LoginUserCommand, AuthResponseDto>,
    ICommandHandler<RefreshTokenCommand, AuthResponseDto>,
    ICommandHandler<RegisterTenantUserCommand, UserDto>
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ISecurityAuditLogRepository _auditLogRepository = auditLogRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IDataProtectorService _dataProtector = dataProtector;
    private readonly ITokenService _tokenService = tokenService;
    private readonly JwtOptions _jwtOptions = jwtOptions ?? new JwtOptions();

    /// <inheritdoc />
    public async Task<Result<AuthResponseDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(command.TenantSlug, cancellationToken);
        if (tenant == null)
        {
            await _auditLogRepository.AddAsync(SecurityAuditLog.Create("LoginFailed_TenantNotFound", tenantId: null, userId: null, command.IpAddress, command.UserAgent), cancellationToken);
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidCredentials);
        }

        if (!tenant.IsActive)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InactiveTenant);
        }

        var blindIndex = _dataProtector.ComputeBlindIndex(command.Email);
        var user = await _userRepository.GetByNormalizedEmailAsync(tenant.Id, blindIndex, cancellationToken);

        if (user == null)
        {
            await _auditLogRepository.AddAsync(SecurityAuditLog.Create("LoginFailed_UserNotFound", tenant.Id, userId: null, command.IpAddress, command.UserAgent), cancellationToken);
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidCredentials);
        }

        var now = DateTimeOffset.UtcNow;
        if (user.IsLockedOut(now))
        {
            await _auditLogRepository.AddAsync(SecurityAuditLog.Create("LoginFailed_AccountLocked", tenant.Id, user.Id, command.IpAddress, command.UserAgent), cancellationToken);
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.UserLockedOut);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InactiveUser);
        }

        if (!_passwordHasher.VerifyPassword(command.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(now);
            await _userRepository.UpdateAsync(user, cancellationToken);
            await _auditLogRepository.AddAsync(SecurityAuditLog.Create("LoginFailed_InvalidPassword", tenant.Id, user.Id, command.IpAddress, command.UserAgent), cancellationToken);
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidCredentials);
        }

        // Reset failed login counter on success
        user.ResetFailedLogin(now);
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Issue tokens
        var plainEmail = _dataProtector.Decrypt(user.EncryptedEmail);
        var accessToken = _tokenService.GenerateAccessToken(user, plainEmail);
        var (refreshTokenStr, refreshTokenHash) = _tokenService.GenerateRefreshToken();

        var refreshTokenEntity = RefreshToken.Create(user.Id, refreshTokenHash, _jwtOptions.RefreshTokenLifetime);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        await _auditLogRepository.AddAsync(SecurityAuditLog.Create("LoginSuccess", tenant.Id, user.Id, command.IpAddress, command.UserAgent), cancellationToken);

        var response = new AuthResponseDto(
            accessToken,
            refreshTokenStr,
            now.Add(_jwtOptions.AccessTokenLifetime),
            user.Id,
            tenant.Id,
            user.PreferredLanguage);

        return Result.Success(response);
    }

    /// <inheritdoc />
    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashRefreshToken(command.RefreshToken);
        var tokenEntity = await _refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);

        if (tokenEntity == null || !tokenEntity.IsActive)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidRefreshToken);
        }

        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InactiveUser);
        }

        var (newRefreshTokenStr, newRefreshTokenHash) = _tokenService.GenerateRefreshToken();
        tokenEntity.Revoke(newRefreshTokenHash);
        await _refreshTokenRepository.UpdateAsync(tokenEntity, cancellationToken);

        var newRefreshTokenEntity = RefreshToken.Create(user.Id, newRefreshTokenHash, _jwtOptions.RefreshTokenLifetime);
        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);

        var plainEmail = _dataProtector.Decrypt(user.EncryptedEmail);
        var accessToken = _tokenService.GenerateAccessToken(user, plainEmail);
        var now = DateTimeOffset.UtcNow;

        await _auditLogRepository.AddAsync(SecurityAuditLog.Create("TokenRefreshed", user.TenantId, user.Id, command.IpAddress), cancellationToken);

        var response = new AuthResponseDto(
            accessToken,
            newRefreshTokenStr,
            now.Add(_jwtOptions.AccessTokenLifetime),
            user.Id,
            user.TenantId,
            user.PreferredLanguage);

        return Result.Success(response);
    }

    /// <inheritdoc />
    public async Task<Result<UserDto>> Handle(RegisterTenantUserCommand command, CancellationToken cancellationToken)
    {
        var existingTenant = await _tenantRepository.GetBySlugAsync(command.TenantSlug, cancellationToken);
        if (existingTenant != null)
        {
            return Result.Failure<UserDto>("Tenant with this slug already exists.", "Tenant.SlugAlreadyExists");
        }

        var tenant = Tenant.Create(command.TenantName, command.TenantSlug);
        await _tenantRepository.AddAsync(tenant, cancellationToken);

        var blindIndex = _dataProtector.ComputeBlindIndex(command.Email);
        var encryptedEmail = _dataProtector.Encrypt(command.Email.Trim().ToLowerInvariant());
        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var user = User.Create(
            tenant.Id,
            blindIndex,
            encryptedEmail,
            passwordHash,
            command.PreferredLanguage);

        await _userRepository.AddAsync(user, cancellationToken);

        await _auditLogRepository.AddAsync(SecurityAuditLog.Create("TenantAndUserRegistered", tenant.Id, user.Id), cancellationToken);

        var dto = new UserDto(
            user.Id,
            tenant.Id,
            command.Email.Trim().ToLowerInvariant(),
            user.PreferredLanguage,
            user.IsActive);

        return Result.Success(dto);
    }
}
