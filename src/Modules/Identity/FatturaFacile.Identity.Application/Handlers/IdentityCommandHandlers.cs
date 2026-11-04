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
public sealed class IdentityCommandHandlers :
    ICommandHandler<LoginUserCommand, AuthResponseDto>,
    ICommandHandler<RefreshTokenCommand, AuthResponseDto>,
    ICommandHandler<RegisterTenantUserCommand, UserDto>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ISecurityAuditLogRepository _auditLogRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDataProtectorService _dataProtector;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public IdentityCommandHandlers(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ISecurityAuditLogRepository auditLogRepository,
        IPasswordHasher passwordHasher,
        IDataProtectorService dataProtector,
        ITokenService tokenService,
        JwtOptions? jwtOptions = null)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _auditLogRepository = auditLogRepository;
        _passwordHasher = passwordHasher;
        _dataProtector = dataProtector;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions ?? new JwtOptions();
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetBySlugAsync(command.TenantSlug, cancellationToken);
        if (tenant == null)
        {
            await _auditLogRepository.AddAsync(SecurityAuditLog.Create("LoginFailed_TenantNotFound", null, null, command.IpAddress, command.UserAgent), cancellationToken);
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
            await _auditLogRepository.AddAsync(SecurityAuditLog.Create("LoginFailed_UserNotFound", tenant.Id, null, command.IpAddress, command.UserAgent), cancellationToken);
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

        // Verify password with Argon2id
        var isPasswordValid = _passwordHasher.VerifyPassword(command.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            user.RecordFailedLogin(now);
            await _userRepository.UpdateAsync(user, cancellationToken);

            var eventName = user.IsLockedOut(now) ? "AccountLocked" : "LoginFailed_BadPassword";
            await _auditLogRepository.AddAsync(SecurityAuditLog.Create(eventName, tenant.Id, user.Id, command.IpAddress, command.UserAgent), cancellationToken);

            return user.IsLockedOut(now)
                ? Result.Failure<AuthResponseDto>(DomainErrors.Identity.UserLockedOut)
                : Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidCredentials);
        }

        // Login success: reset counter
        user.ResetFailedLogin(now);
        await _userRepository.UpdateAsync(user, cancellationToken);

        var plainEmail = _dataProtector.Decrypt(user.EncryptedEmail);
        var accessToken = _tokenService.GenerateAccessToken(user, plainEmail);

        var (refreshTokenString, refreshTokenHash) = _tokenService.GenerateRefreshToken();
        var refreshTokenEntity = RefreshToken.Create(user.Id, refreshTokenHash, _jwtOptions.RefreshTokenLifetime);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        await _auditLogRepository.AddAsync(SecurityAuditLog.Create("LoginSuccess", tenant.Id, user.Id, command.IpAddress, command.UserAgent), cancellationToken);

        var response = new AuthResponseDto(
            accessToken,
            refreshTokenString,
            DateTimeOffset.UtcNow.Add(_jwtOptions.AccessTokenLifetime),
            user.Id,
            user.TenantId,
            user.PreferredLanguage);

        return Result.Success(response);
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashRefreshToken(command.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);

        if (existingToken == null || !existingToken.IsActive)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidRefreshToken);
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return Result.Failure<AuthResponseDto>(DomainErrors.Identity.InvalidRefreshToken);
        }

        // Rotate token
        var (newRefreshTokenString, newRefreshTokenHash) = _tokenService.GenerateRefreshToken();
        existingToken.Revoke(newRefreshTokenHash);
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        var newRefreshTokenEntity = RefreshToken.Create(user.Id, newRefreshTokenHash, _jwtOptions.RefreshTokenLifetime);
        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);

        var plainEmail = _dataProtector.Decrypt(user.EncryptedEmail);
        var accessToken = _tokenService.GenerateAccessToken(user, plainEmail);

        var response = new AuthResponseDto(
            accessToken,
            newRefreshTokenString,
            DateTimeOffset.UtcNow.Add(_jwtOptions.AccessTokenLifetime),
            user.Id,
            user.TenantId,
            user.PreferredLanguage);

        return Result.Success(response);
    }

    public async Task<Result<UserDto>> Handle(RegisterTenantUserCommand command, CancellationToken cancellationToken)
    {
        var existingTenant = await _tenantRepository.GetBySlugAsync(command.TenantSlug, cancellationToken);
        var tenant = existingTenant ?? Tenant.Create(command.TenantName, command.TenantSlug);

        if (existingTenant == null)
        {
            await _tenantRepository.AddAsync(tenant, cancellationToken);
        }

        var blindIndex = _dataProtector.ComputeBlindIndex(command.Email);
        var existingUser = await _userRepository.GetByNormalizedEmailAsync(tenant.Id, blindIndex, cancellationToken);
        if (existingUser != null)
        {
            return Result.Failure<UserDto>(DomainErrors.Identity.EmailAlreadyExists);
        }

        var encryptedEmail = _dataProtector.Encrypt(command.Email);
        var passwordHash = _passwordHasher.HashPassword(command.Password);

        var user = User.Create(
            tenant.Id,
            blindIndex,
            encryptedEmail,
            passwordHash,
            command.PreferredLanguage);

        await _userRepository.AddAsync(user, cancellationToken);

        return Result.Success(new UserDto(user.Id, user.TenantId, command.Email, user.PreferredLanguage, user.IsActive));
    }
}
