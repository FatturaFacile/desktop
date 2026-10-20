using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.DTOs;

public record AbsenceDto(
    Guid Id,
    Guid TenantId,
    Guid EmployeeId,
    AbsenceType AbsenceType,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalWorkingDays,
    bool IsApproved,
    string? CertificateNumber);