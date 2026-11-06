// <copyright file="DeletePayrollRecordCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Handler for <see cref="DeletePayrollRecordCommand{TEntity}"/>: system wage types and employees with payslips are protected.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="DeletePayrollRecordCommandHandler{TEntity}"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class DeletePayrollRecordCommandHandler<TEntity>(IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePayrollRecordCommand<TEntity>, bool>
    where TEntity : class, IBaseEntity, new()
{
    /// <inheritdoc/>
    public Task<bool> Handle(DeletePayrollRecordCommand<TEntity> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.Id, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.Repository<TEntity>();
        var entity = await repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (entity is null)
        {
            return false;
        }

        if (entity is PayrollWageType { IsSystem: true })
        {
            throw new InvalidOperationException("I generi salariali di sistema non possono essere eliminati (disattivarli se non servono).");
        }

        if (entity is HrEmployee employee)
        {
            var payslips = await unitOfWork.Repository<Payslip>().FindAsync(p => p.EmployeeId == employee.Id, cancellationToken).ConfigureAwait(false);

            if (payslips.Count > 0)
            {
                throw new InvalidOperationException("Il dipendente ha già buste paga: registrare la data di uscita invece di eliminarlo.");
            }
        }

        repository.Delete(entity);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
