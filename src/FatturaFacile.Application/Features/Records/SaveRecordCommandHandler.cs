// <copyright file="SaveRecordCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Domain.Common;
using MediatR;

namespace FatturaFacile.Application.Features.Records;

/// <summary>
/// Handler for <see cref="SaveRecordCommand{TEntity}"/>: new shared-capable records are assigned to the current company.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="SaveRecordCommandHandler{TEntity}"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current company.</param>
public sealed class SaveRecordCommandHandler<TEntity>(IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService)
    : IRequestHandler<SaveRecordCommand<TEntity>, Guid>
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Task<Guid> Handle(SaveRecordCommand<TEntity> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Record is null
            ? throw new ArgumentException("Il record da salvare è obbligatorio.", nameof(request))
            : HandleInternalAsync(request.Record, cancellationToken);
    }

    private async Task<Guid> HandleInternalAsync(TEntity record, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<TEntity>();
        var existing = record.Id == Guid.Empty ? null : await repository.GetByIdAsync(record.Id, cancellationToken).ConfigureAwait(false);

        if (existing is null)
        {
            var created = RecordCopier.Clone(record);
            created.Id = created.Id == Guid.Empty ? Guid.NewGuid() : created.Id;
            created.CreatedAt = DateTime.UtcNow;

            if (created is IOptionalTenantEntity optional)
            {
                optional.TenantId = currentTenantService.TenantId;
            }

            await repository.AddAsync(created, cancellationToken).ConfigureAwait(false);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return created.Id;
        }

        RecordCopier.CopyValues(record, existing);
        existing.UpdatedAt = DateTime.UtcNow;
        repository.Update(existing);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return existing.Id;
    }
}
