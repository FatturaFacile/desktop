using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

/// <summary>
/// Represents login command.
/// </summary>
/// <param name="Username">The username.</param>
/// <param name="Password">The password.</param>
/// <param name="TenantId">The tenant id.</param>
public record LoginCommand(
    string Username,
    string Password,
    Guid TenantId) : IRequest<UserDto>;
