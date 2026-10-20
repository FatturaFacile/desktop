using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

public record LoginCommand(
    string Username,
    string Password,
    Guid TenantId) : IRequest<UserDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        _ = RuleFor(x => x.Username).NotEmpty();
        _ = RuleFor(x => x.Password).NotEmpty();
        _ = RuleFor(x => x.TenantId).NotEmpty();
    }
}

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

public record GetTenantsQuery() : IRequest<IReadOnlyList<TenantDto>>;

public sealed class GetTenantsQueryHandler(ITenantRepository tenantRepository, IMapper mapper) : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<TenantDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetActiveTenantsAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TenantDto>>(tenants);
    }
}

public record SwitchTenantCommand(Guid TargetTenantId) : IRequest<TenantDto>;

public sealed class SwitchTenantCommandHandler(ITenantRepository tenantRepository, ITenantProvider tenantProvider, IMapper mapper) : IRequestHandler<SwitchTenantCommand, TenantDto>
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly IMapper _mapper = mapper;

    public async Task<TenantDto> Handle(SwitchTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TargetTenantId, cancellationToken)
            ?? throw new InvalidOperationException("Tenant not found.");

        _tenantProvider.SetTenant(tenant.Id, tenant.Code);
        return _mapper.Map<TenantDto>(tenant);
    }
}
