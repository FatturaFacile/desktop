// <copyright file="GetLipeOverviewQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="GetLipeOverviewQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetLipeOverviewQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetLipeOverviewQuery, IReadOnlyList<LipeOverviewRow>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<LipeOverviewRow>> Handle(GetLipeOverviewQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Year, cancellationToken);
    }

    private async Task<IReadOnlyList<LipeOverviewRow>> HandleInternalAsync(int year, CancellationToken cancellationToken)
    {
        var returns = await unitOfWork.Repository<VatReturn>().FindAsync(r => r.Country == "IT" && r.PeriodFrom.Year == year, cancellationToken).ConfigureAwait(false);
        var submissions = await unitOfWork.Repository<LipeSubmission>().FindAsync(s => s.Year == year, cancellationToken).ConfigureAwait(false);

        return [.. returns.OrderBy(static r => r.PeriodFrom).Select(r => new LipeOverviewRow(r, submissions.Where(s => s.VatReturnId == r.Id).OrderByDescending(static s => s.CreatedAt).FirstOrDefault()))];
    }
}
