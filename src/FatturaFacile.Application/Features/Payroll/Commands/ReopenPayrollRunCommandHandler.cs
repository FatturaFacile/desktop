// <copyright file="ReopenPayrollRunCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Handler for <see cref="ReopenPayrollRunCommand"/>: only the most recent closed run can be reopened, so that the
/// cumulated ceilings of the following months stay consistent.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ReopenPayrollRunCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class ReopenPayrollRunCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<ReopenPayrollRunCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(ReopenPayrollRunCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request.RunId, cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(Guid runId, CancellationToken cancellationToken)
    {
        var runs = await unitOfWork.Repository<PayrollRun>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);
        var run = runs.FirstOrDefault(r => r.Id == runId);

        if (run is null || !string.Equals(run.Status, "Final", StringComparison.Ordinal))
        {
            return false;
        }

        var later = runs.Any(r => (r.Year * 12) + r.Month > (run.Year * 12) + run.Month);

        if (later)
        {
            throw new InvalidOperationException("Esistono elaborazioni successive: si può riaprire solo l'ultima elaborazione.");
        }

        run.Status = "Draft";
        run.FinalizedAt = null;
        unitOfWork.Repository<PayrollRun>().Update(run);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }
}
