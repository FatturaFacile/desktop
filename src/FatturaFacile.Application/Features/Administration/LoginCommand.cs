using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

public record LoginCommand(
    string Username,
    string Password,
    Guid TenantId) : IRequest<UserDto>;