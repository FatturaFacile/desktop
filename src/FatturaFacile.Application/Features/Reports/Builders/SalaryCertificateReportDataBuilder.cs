// <copyright file="SalaryCertificateReportDataBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.Common.Reporting;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Features.Reports.Builders;

/// <summary>
/// Builds the merge fields of a salary certificate (form 11): the boxes as fields "Box1" … "Box13".
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SalaryCertificateReportDataBuilder"/> class.
/// </remarks>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class SalaryCertificateReportDataBuilder(IUnitOfWork unitOfWork) : IReportDataBuilder
{
    /// <inheritdoc/>
    public ReportDocumentType DocumentType => ReportDocumentType.SalaryCertificate;

    /// <inheritdoc/>
    public Task<ReportData?> BuildAsync(Guid entityId, CultureInfo culture, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(culture);

        return BuildInternalAsync(entityId, culture, cancellationToken);
    }

    private async Task<ReportData?> BuildInternalAsync(Guid entityId, CultureInfo culture, CancellationToken cancellationToken)
    {
        var certificate = await unitOfWork.Repository<SalaryCertificate>().GetByIdAsync(entityId, cancellationToken).ConfigureAwait(false);

        if (certificate is null)
        {
            return null;
        }

        var employee = await unitOfWork.Repository<HrEmployee>().GetByIdAsync(certificate.EmployeeId, cancellationToken).ConfigureAwait(false);
        var tenant = await unitOfWork.Repository<Tenant>().GetByIdAsync(certificate.TenantId, cancellationToken).ConfigureAwait(false);

        ReportData data = new()
        {
            SuggestedFileName = ReportFormat.SafeFileName(string.Create(CultureInfo.InvariantCulture, $"Certificato_salario_{certificate.Year}_{employee?.FullName}.pdf")),
        };

        PayslipReportDataBuilder.AddCompanyAndEmployee(data, tenant, employee, culture);
        data.Fields["Year"] = certificate.Year.ToString(CultureInfo.InvariantCulture);
        data.Fields["PeriodFrom"] = ReportFormat.Date(certificate.PeriodFrom, culture);
        data.Fields["PeriodTo"] = ReportFormat.Date(certificate.PeriodTo, culture);
        data.Fields["Box1"] = ReportFormat.Amount(certificate.Box1Salary, culture);
        data.Fields["Box2"] = ReportFormat.OptionalAmount(certificate.Box2Benefits, culture);
        data.Fields["Box3"] = ReportFormat.OptionalAmount(certificate.Box3Irregular, culture);
        data.Fields["Box4"] = ReportFormat.OptionalAmount(certificate.Box4Capital, culture);
        data.Fields["Box7"] = ReportFormat.OptionalAmount(certificate.Box7Other, culture);
        data.Fields["Box8"] = ReportFormat.Amount(certificate.Box8Gross, culture);
        data.Fields["Box9"] = ReportFormat.Amount(certificate.Box9SocialContributions, culture);
        data.Fields["Box10"] = ReportFormat.OptionalAmount(certificate.Box10Pension, culture);
        data.Fields["Box11"] = ReportFormat.Amount(certificate.Box11Net, culture);
        data.Fields["Box12"] = ReportFormat.OptionalAmount(certificate.Box12WithholdingTax, culture);
        data.Fields["Box13"] = ReportFormat.OptionalAmount(certificate.Box13Expenses, culture);
        data.Fields["Remarks"] = certificate.Remarks ?? string.Empty;
        data.Fields["IssueDate"] = ReportFormat.Date(DateTime.Today, culture);
        return data;
    }
}
