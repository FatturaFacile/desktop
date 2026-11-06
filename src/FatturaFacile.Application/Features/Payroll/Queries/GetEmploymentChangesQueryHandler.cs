// <copyright file="GetEmploymentChangesQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.DTOs;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.Payroll.Queries;

/// <summary>
/// Handler for <see cref="GetEmploymentChangesQuery"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GetEmploymentChangesQueryHandler"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetEmploymentChangesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetEmploymentChangesQuery, IReadOnlyList<EmploymentChangeDto>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<EmploymentChangeDto>> Handle(GetEmploymentChangesQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private static bool InMonth(DateTime date, GetEmploymentChangesQuery request) => date.Year == request.Year && date.Month == request.Month;

    private static string Recipients(HrEmployee employee, bool entry)
    {
        List<string> recipients = ["Cassa di compensazione AVS", "Assicurazione infortuni (LAINF)", "Cassa pensione (LPP)"];

        if (employee.IsWithholdingTaxSubject)
        {
            recipients.Add(entry ? "Ufficio imposta alla fonte (entro 8 giorni)" : "Ufficio imposta alla fonte (uscita)");
        }

        if (entry && string.Equals(employee.ResidencePermit, "G", StringComparison.OrdinalIgnoreCase))
        {
            recipients.Add("Ufficio della migrazione (permesso G)");
        }

        return string.Join(", ", recipients);
    }

    private async Task<IReadOnlyList<EmploymentChangeDto>> HandleInternalAsync(GetEmploymentChangesQuery request, CancellationToken cancellationToken)
    {
        var employees = await unitOfWork.Repository<HrEmployee>().FindAsync(static _ => true, cancellationToken).ConfigureAwait(false);

        List<EmploymentChangeDto> changes = [];

        foreach (var employee in employees)
        {
            if (InMonth(employee.HireDate, request))
            {
                changes.Add(new EmploymentChangeDto(employee.FullName, "Entrata", employee.HireDate, employee.SocialSecurityNumber, employee.ResidencePermit, employee.IsWithholdingTaxSubject, Recipients(employee, entry: true)));
            }

            if (employee.TerminationDate is { } exit && InMonth(exit, request))
            {
                changes.Add(new EmploymentChangeDto(employee.FullName, "Uscita", exit, employee.SocialSecurityNumber, employee.ResidencePermit, employee.IsWithholdingTaxSubject, Recipients(employee, entry: false)));
            }
        }

        return [.. changes.OrderBy(static c => c.Date)];
    }
}
