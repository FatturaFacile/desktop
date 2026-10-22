// <copyright file="TimeSheetProfile.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using AutoMapper;

using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.TimeTracking;

namespace FatturaFacile.Application.Mappings;

/// <summary>
/// AutoMapper profile configuring mappings for time attendance entries and absence records to DTOs.
/// </summary>
public sealed class TimeSheetProfile : Profile
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSheetProfile"/> class, setting projection expressions.
    /// </summary>
    public TimeSheetProfile()
    {
        _ = CreateMap<TimeEntry, TimeEntryDto>()
           .ForCtorParam(
                         ctorParamName: nameof(TimeEntryDto.EmployeeName),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => $"Employee #{src.EmployeeId.ToString().Substring(0, 6)}"))
           .ForCtorParam(
                         ctorParamName: nameof(TimeEntryDto.TotalNetHours),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.TotalNetHours));

        _ = CreateMap<AbsenceRecord, AbsenceDto>()
           .ForCtorParam(
                         ctorParamName: nameof(AbsenceDto.CertificateNumber),
                         paramOptions: opt => opt.MapFrom(sourceMember: src => src.OfficialCertificateNumber));
    }

}
