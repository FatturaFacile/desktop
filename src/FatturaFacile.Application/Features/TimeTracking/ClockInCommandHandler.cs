using AutoMapper;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities.TimeTracking;
using FatturaFacile.Domain.Repositories;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking;

/// <summary>
/// Represents clock in command handler.
/// </summary>
/// <param name="repository">The repository.</param>
/// <param name="tenantProvider">The tenant provider.</param>
/// <param name="mapper">The mapper.</param>
public sealed class ClockInCommandHandler(ITimeEntryRepository repository, ITenantProvider tenantProvider, IMapper mapper) : IRequestHandler<ClockInCommand, TimeEntryDto>
{
    private readonly ITimeEntryRepository _repository = repository;

    private readonly ITenantProvider _tenantProvider = tenantProvider;

    private readonly IMapper _mapper = mapper;

/// <summary>
    /// Handle.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a processing or validation error occurs.</exception>
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
