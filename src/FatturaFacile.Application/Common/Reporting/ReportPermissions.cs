// <copyright file="ReportPermissions.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Enums;

namespace FatturaFacile.Application.Common.Reporting;

/// <summary>
/// The permissions needed to print or manage the templates of a document type.
/// </summary>
public static class ReportPermissions
{
    /// <summary>
    /// Returns the permission needed to print documents of a type.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <returns>The read permission.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the document type is unknown.</exception>
    public static Permission Read(ReportDocumentType documentType) => documentType switch
    {
        ReportDocumentType.Invoice => Permission.CommercialRead,
        ReportDocumentType.Payslip => Permission.PayrollRead,
        ReportDocumentType.SalaryCertificate => Permission.PayrollRead,
        ReportDocumentType.TimeSheet => Permission.TimeRead,
        ReportDocumentType.LabourBook => Permission.PayrollRead,
        _ => throw new ArgumentOutOfRangeException(nameof(documentType)),
    };

    /// <summary>
    /// Returns the permission needed to add templates for a document type.
    /// </summary>
    /// <param name="documentType">The document type.</param>
    /// <returns>The write permission.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When the document type is unknown.</exception>
    public static Permission Write(ReportDocumentType documentType) => documentType switch
    {
        ReportDocumentType.Invoice => Permission.InvoicesWrite,
        ReportDocumentType.Payslip => Permission.PayrollWrite,
        ReportDocumentType.SalaryCertificate => Permission.PayrollWrite,
        ReportDocumentType.TimeSheet => Permission.TimeManage,
        ReportDocumentType.LabourBook => Permission.PayrollWrite,
        _ => throw new ArgumentOutOfRangeException(nameof(documentType)),
    };
}
