using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

public record UserDto(
    Guid Id,
    Guid TenantId,
    string Username,
    string Email,
    string FullName,
    UserRole Role,
    bool IsActive,
    DateTime? LastLoginUtc);