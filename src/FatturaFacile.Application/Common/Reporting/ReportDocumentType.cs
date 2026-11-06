// <copyright file="ReportDocumentType.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Reporting;

/// <summary>
/// The kinds of printable documents; each has its own template folder on the file system.
/// </summary>
public enum ReportDocumentType
{
    /// <summary>
    /// Customer invoices (folder "Invoice").
    /// </summary>
    Invoice = 0,

    /// <summary>
    /// Monthly payslips (folder "Payslip").
    /// </summary>
    Payslip = 1,

    /// <summary>
    /// Swiss salary certificates, form 11 (folder "SalaryCertificate").
    /// </summary>
    SalaryCertificate = 2,

    /// <summary>
    /// Monthly working time sheets (folder "TimeSheet").
    /// </summary>
    TimeSheet = 3,
}
