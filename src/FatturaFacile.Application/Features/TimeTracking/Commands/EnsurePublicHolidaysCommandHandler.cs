// <copyright file="EnsurePublicHolidaysCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.TimeTracking;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.TimeTracking.Commands;

/// <summary>
/// Handler for <see cref="EnsurePublicHolidaysCommand"/>: the canton is the default work canton of the payroll settings.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EnsurePublicHolidaysCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class EnsurePublicHolidaysCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<EnsurePublicHolidaysCommand, int>
{
    /// <inheritdoc/>
    public Task<int> Handle(EnsurePublicHolidaysCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<int> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var from = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var to = from.AddYears(1);
        var existing = await unitOfWork.Repository<PublicHoliday>().FindAsync(h => h.Date >= from && h.Date < to, cancellationToken).ConfigureAwait(false);

        if (existing.Count > 0)
        {
            return 0;
        }

        var settings = (await unitOfWork.Repository<SwissPayrollSettings>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false)).FirstOrDefault();
        var canton = settings?.DefaultWorkCanton ?? "TI";
        var holidays = SwissHolidayCalendar.ForCanton(canton, year);

        foreach (var (date, name) in holidays)
        {
            await unitOfWork.Repository<PublicHoliday>().AddAsync(new PublicHoliday { Date = date, Name = name, Canton = canton }, cancellationToken).ConfigureAwait(false);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return holidays.Count;
    }
}
