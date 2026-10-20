using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    ITenantProvider tenantProvider,
    ICurrentUserService currentUserService,
    IPasswordHasher passwordHasher,
    IMapper mapper) : IRequestHandler<LoginCommand, UserDto>
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IMapper _mapper = mapper;

    public async Task<UserDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken)
                     ?? throw new UnauthorizedAccessException("Selected tenant does not exist or is inactive.");

        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is locked or deactivated.");
        }

        var isValid = _passwordHasher.VerifyHash(request.Password, user.PasswordHash, user.PasswordSalt);
        if (!isValid)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        user.RecordLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        _tenantProvider.SetTenant(tenant.Id, tenant.Code);
        _currentUserService.SetUser(user.Id, user.Username, user.FullName, user.Role.ToString());

        return _mapper.Map<UserDto>(user);
    }
}