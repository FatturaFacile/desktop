// <copyright file="GetPayrollRecordsQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Handler for <see cref="GetPayrollRecordsQuery{TEntity}"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="GetPayrollRecordsQueryHandler{TEntity}"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetPayrollRecordsQueryHandler<TEntity>(IUnitOfWork unitOfWork)
    : IRequestHandler<GetPayrollRecordsQuery<TEntity>, IReadOnlyList<TEntity>>
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<TEntity>> Handle(GetPayrollRecordsQuery<TEntity> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private async Task<IReadOnlyList<TEntity>> HandleInternalAsync(GetPayrollRecordsQuery<TEntity> request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<TEntity>();
        var records = request.Filter is null
            ? await repository.FindAsync(static _ => true, cancellationToken).ConfigureAwait(false)
            : await repository.FindAsync(request.Filter, cancellationToken).ConfigureAwait(false);

        return [.. records.Select(RecordCopier.Clone)];
    }
}
