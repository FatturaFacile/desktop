// <copyright file="EnsureProcessingActivitiesCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Privacy;

/// <summary>
/// Handles <see cref="EnsureProcessingActivitiesCommand"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class EnsureProcessingActivitiesCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<EnsureProcessingActivitiesCommand, int>
{
    /// <inheritdoc />
    public async Task<int> Handle(EnsureProcessingActivitiesCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<ProcessingActivity>();

        if ((await repository.FindAsync(static _ => true, cancellationToken).ConfigureAwait(false)).Count > 0)
        {
            return 0;
        }

        var activities = DefaultProcessingActivities.Create();

        foreach (var activity in activities)
        {
            await repository.AddAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return activities.Count;
    }
}
