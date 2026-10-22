// <copyright file="Report.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile;

/// <summary>
/// Predefined DevExpress XtraReport template rendering tabular data and summary information.
/// </summary>
public partial class Report : DevExpress.XtraReports.UI.XtraReport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Report"/> class and sets up report bands, controls, and bindings.
    /// </summary>
    public Report()
    {
        InitializeComponent();
    }
}
