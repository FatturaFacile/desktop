// <copyright file="EnsureSwissPayrollSetupCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Features.Payroll.Validators;
using FatturaFacile.Application.Payroll.Swiss;
using FatturaFacile.Domain.Common;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Commands;

/// <summary>
/// Handler for <see cref="EnsureSwissPayrollSetupCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="EnsureSwissPayrollSetupCommandHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
/// <param name="currentTenantService">The current company.</param>
public sealed class EnsureSwissPayrollSetupCommandHandler(IUnitOfWork unitOfWork, ICurrentTenantService currentTenantService)
    : IRequestHandler<EnsureSwissPayrollSetupCommand, bool>
{
    /// <inheritdoc/>
    public Task<bool> Handle(EnsureSwissPayrollSetupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(cancellationToken);
    }

    private async Task<bool> HandleInternalAsync(CancellationToken cancellationToken)
    {
        var tenantId = currentTenantService.TenantId;
        var changed = false;

        var settings = await unitOfWork.Repository<SwissPayrollSettings>().FindAsync(s => s.TenantId == tenantId, cancellationToken).ConfigureAwait(false);

        if (settings.Count == 0)
        {
            await unitOfWork.Repository<SwissPayrollSettings>().AddAsync(new SwissPayrollSettings { TenantId = tenantId }, cancellationToken).ConfigureAwait(false);
            changed = true;
        }

        var wageTypes = await unitOfWork.Repository<PayrollWageType>().FindAsync(w => w.TenantId == tenantId, cancellationToken).ConfigureAwait(false);
        var existingCodes = wageTypes.Select(static w => w.Code).ToHashSet(StringComparer.Ordinal);

        foreach (var wageType in SwissWageTypeCatalog.Create().Where(w => !existingCodes.Contains(w.Code)))
        {
            wageType.TenantId = tenantId;
            await unitOfWork.Repository<PayrollWageType>().AddAsync(wageType, cancellationToken).ConfigureAwait(false);
            changed = true;
        }

        var rates = await unitOfWork.Repository<FamilyAllowanceRate>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);

        if (rates.Count == 0)
        {
            // Federal minimum amounts (CHF 215 / 268 from 2025) as editable starting values: check the cantonal amounts.
            var validFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            foreach (var canton in PayrollValidation.Cantons)
            {
                await unitOfWork.Repository<FamilyAllowanceRate>().AddAsync(
                    new FamilyAllowanceRate { Canton = canton, ValidFrom = validFrom, ChildAllowance = 215m, EducationAllowance = 268m },
                    cancellationToken).ConfigureAwait(false);
            }

            changed = true;
        }

        if (changed)
        {
            _ = await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        return changed;
    }
}
