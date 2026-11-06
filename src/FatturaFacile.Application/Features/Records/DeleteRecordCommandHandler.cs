// <copyright file="DeleteRecordCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Records;

/// <summary>
/// Handler for <see cref="DeleteRecordCommand{TEntity}"/>: posted documents cannot be deleted and entities with a
/// DeletedAt column are deleted logically.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="DeleteRecordCommandHandler{TEntity}"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class DeleteRecordCommandHandler<TEntity>(IUnitOfWork unitOfWork) : IRequestHandler<DeleteRecordCommand<TEntity>, bool>
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Task<bool> Handle(DeleteRecordCommand<TEntity> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Id, cancellationToken);
    }

    private static bool IsLocked(TEntity entity) => entity switch
    {
        PurchaseInvoice invoice => !string.Equals(invoice.Status, "Draft", StringComparison.Ordinal),
        VatReturn vatReturn => string.Equals(vatReturn.Status, "Final", StringComparison.Ordinal),
        GeneralLedgerEntry entry => entry.IsLocked,
        _ => false,
    };

    private async Task<bool> HandleInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<TEntity>();
        var entity = await repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        if (IsLocked(entity))
        {
            throw new InvalidOperationException("Il documento è registrato o chiuso e non può essere eliminato: stornarlo con una registrazione inversa.");
        }

        var deletedAt = typeof(TEntity).GetProperty("DeletedAt");

        if (deletedAt?.PropertyType == typeof(DateTime?))
        {
            deletedAt.SetValue(entity, DateTime.UtcNow);
            repository.Update(entity);
        }
        else
        {
            repository.Delete(entity);
        }

        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
