// <copyright file="FinalizePayrollRunCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Handler for <see cref="FinalizePayrollRunCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FinalizePayrollRunCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class FinalizePayrollRunCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<FinalizePayrollRunCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(FinalizePayrollRunCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.RunId, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(Guid runId, CancellationToken cancellationToken)
    {
        var run = await unitOfWork.Repository<PayrollRun>().GetByIdAsync(runId, cancellationToken).ConfigureAwait(false);

        if (run is null || string.Equals(run.Status, "Final", StringComparison.Ordinal))
        {
            return false;
        }

        var payslips = await unitOfWork.Repository<Payslip>().FindAsync(p => p.PayrollRunId == runId, cancellationToken).ConfigureAwait(false);

        if (payslips.Count == 0)
        {
            throw new InvalidOperationException("Calcolare l'elaborazione prima di chiuderla.");
        }

        run.Status = "Final";
        run.FinalizedAt = DateTime.UtcNow;
        unitOfWork.Repository<PayrollRun>().Update(run);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
