// <copyright file="GetRecordsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Application.Features.Records;

/// <summary>
/// Handler for <see cref="GetRecordsQuery{TEntity}"/>: logically deleted records are excluded.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="GetRecordsQueryHandler{TEntity}"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetRecordsQueryHandler<TEntity>(IUnitOfWork unitOfWork) : IRequestHandler<GetRecordsQuery<TEntity>, IReadOnlyList<TEntity>>
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<TEntity>> Handle(GetRecordsQuery<TEntity> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<IReadOnlyList<TEntity>> HandleInternalAsync(GetRecordsQuery<TEntity> request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<TEntity>();
        var records = request.Filter is null
            ? await repository.FindAsync(static _ => true, cancellationToken).ConfigureAwait(false)
            : await repository.FindAsync(request.Filter, cancellationToken).ConfigureAwait(false);
        var deletedAt = typeof(TEntity).GetProperty("DeletedAt");

        return [.. records.Where(r => deletedAt?.GetValue(r) is null).Select(RecordCopier.Clone)];
    }
}
