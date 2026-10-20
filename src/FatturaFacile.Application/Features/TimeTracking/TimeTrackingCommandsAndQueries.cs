using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Enums;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

public record ClockInCommand(
    Guid EmployeeId,
    RecordingType RecordingType = RecordingType.Detailed,
    double? Latitude = null,
    double? Longitude = null,
    string? BadgeSerial = null) : IRequest<TimeEntryDto>;

public sealed class ClockInCommandHandler(ITimeEntryRepository repository, ITenantProvider tenantProvider, IMapper mapper) : IRequestHandler<ClockInCommand, TimeEntryDto>
{
    private readonly ITimeEntryRepository _repository = repository;
    private readonly ITenantProvider _tenantProvider = tenantProvider;
    private readonly IMapper _mapper = mapper;

    public async Task<TimeEntryDto> Handle(ClockInCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetActiveClockInAsync(request.EmployeeId, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException("Employee already has an active clock-in session.");
        }

        var entry = TimeEntry.ClockIn(
            _tenantProvider.CurrentTenantId,
            request.EmployeeId,
            DateTime.UtcNow,
            request.RecordingType,
            request.Latitude,
            request.Longitude,
            request.BadgeSerial);

        await _repository.AddAsync(entry, cancellationToken);
        return _mapper.Map<TimeEntryDto>(entry);
    }
}

public record ClockOutCommand(
    Guid EmployeeId,
    int BreakMinutes = 0) : IRequest<TimeEntryDto>;

public sealed class ClockOutCommandHandler(ITimeEntryRepository repository, IMapper mapper) : IRequestHandler<ClockOutCommand, TimeEntryDto>
{
    private readonly ITimeEntryRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<TimeEntryDto> Handle(ClockOutCommand request, CancellationToken cancellationToken)
    {
        var active = await _repository.GetActiveClockInAsync(request.EmployeeId, cancellationToken)
            ?? throw new InvalidOperationException("No active clock-in session found for this employee.");

        active.ClockOut(DateTime.UtcNow, request.BreakMinutes);
        await _repository.UpdateAsync(active, cancellationToken);
        return _mapper.Map<TimeEntryDto>(active);
    }
}

public record GetTimeEntriesQuery(Guid? EmployeeId = null) : IRequest<IReadOnlyList<TimeEntryDto>>;

public sealed class GetTimeEntriesQueryHandler(ITimeEntryRepository repository, IMapper mapper) : IRequestHandler<GetTimeEntriesQuery, IReadOnlyList<TimeEntryDto>>
{
    private readonly ITimeEntryRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<IReadOnlyList<TimeEntryDto>> Handle(GetTimeEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = request.EmployeeId.HasValue
            ? await _repository.GetEntriesByEmployeeAndDateRangeAsync(
                request.EmployeeId.Value,
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow,
                cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<TimeEntryDto>>(entries);
    }
}
