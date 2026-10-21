// <copyright file="TimeSheetProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.TimeTracking;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// Represents time sheet profile.
/// </summary>
public sealed class TimeSheetProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSheetProfile"/> class.
    /// </summary>
    public TimeSheetProfile()
    {
        _ = CreateMap<TimeEntry, TimeEntryDto>()
            .ForCtorParam(
                nameof(TimeEntryDto.EmployeeName),
                opt => opt.MapFrom(src => $"Employee #{src.EmployeeId.ToString().Substring(0, 6)}"))
            .ForCtorParam(
                nameof(TimeEntryDto.TotalNetHours),
                opt => opt.MapFrom(src => src.TotalNetHours));

        _ = CreateMap<AbsenceRecord, AbsenceDto>()
            .ForCtorParam(
                nameof(AbsenceDto.CertificateNumber),
                opt => opt.MapFrom(src => src.OfficialCertificateNumber));
    }
}
