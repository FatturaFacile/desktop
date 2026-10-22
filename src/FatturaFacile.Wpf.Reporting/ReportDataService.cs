// <copyright file="ReportDataService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Reporting;

/// <summary>
/// Dummy data provider for XtraReports ObjectDataSource bindings.
/// </summary>
public static class ReportDataService
{
    /// <summary>
    /// Gets sample country data used for report designer preview and data schema binding.
    /// </summary>
    /// <returns>An array containing mock country data records.</returns>
    public static object[] GetCountryData() => [];
}
