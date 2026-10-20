using AutoMapper;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

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