using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

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