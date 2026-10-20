using FatturaFacile.Application.DTOs;
using MediatR;

namespace FatturaFacile.Application.Features.Administration;

public record SwitchTenantCommand(Guid TargetTenantId) : IRequest<TenantDto>;