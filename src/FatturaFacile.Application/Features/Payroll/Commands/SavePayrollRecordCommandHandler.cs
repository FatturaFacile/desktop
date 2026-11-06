// <copyright file="SavePayrollRecordCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Records;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Handler for <see cref="SavePayrollRecordCommand{TEntity}"/>.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="SavePayrollRecordCommandHandler{TEntity}"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class SavePayrollRecordCommandHandler<TEntity>(IUnitOfWork unitOfWork)
    : IRequestHandler<SavePayrollRecordCommand<TEntity>, Guid>
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Task<Guid> Handle(SavePayrollRecordCommand<TEntity> request, CancellationToken cancellationToken)
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

            if (created.Id == Guid.Empty)
            {
                created.Id = Guid.NewGuid();
            }

            created.CreatedAt = DateTime.UtcNow;
            await repository.AddAsync(created, cancellationToken).ConfigureAwait(false);
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return created.Id;
        }

        if (existing is PayrollWageType { IsSystem: true } systemType && record is PayrollWageType edited
            && !string.Equals(systemType.Code, edited.Code, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Il codice di un genere salariale di sistema non può essere modificato.");
        }

        RecordCopier.CopyValues(record, existing);
        existing.UpdatedAt = DateTime.UtcNow;
        repository.Update(existing);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return existing.Id;
    }
}
