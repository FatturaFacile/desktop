using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

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