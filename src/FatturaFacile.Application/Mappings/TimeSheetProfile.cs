using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.TimeTracking;

namespace FatturaFacile.Application.Mappings;

public sealed class TimeSheetProfile : Profile
{
    public TimeSheetProfile()
    {
        _ = CreateMap<TimeEntry, TimeEntryDto>()
            .ForCtorParam(nameof(TimeEntryDto.EmployeeName), opt => opt.MapFrom(src => $"Employee #{src.EmployeeId.ToString().Substring(0, 6)}"))
            .ForCtorParam(nameof(TimeEntryDto.TotalNetHours), opt => opt.MapFrom(src => src.TotalNetHours));

        _ = CreateMap<AbsenceRecord, AbsenceDto>()
            .ForCtorParam(nameof(AbsenceDto.CertificateNumber), opt => opt.MapFrom(src => src.OfficialCertificateNumber));
    }
}
