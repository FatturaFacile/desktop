// <copyright file="ReportingModuleInitializer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.ViewModels;
using FatturaFacile.Views;
using FatturaFacile.Wpf.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FatturaFacile.Wpf.Reporting;

/// <summary>
/// Registers reporting services, views, and viewmodels into dependency injection.
/// </summary>
public sealed class ReportingModuleInitializer : IModuleInitializer
{
    /// <inheritdoc />
    public void Initialize(IServiceCollection services, IConfiguration configuration)
    {
        // ViewModels
        _ = services.AddTransient<ReportViewModel>();
        _ = services.AddTransient<ReportDesignerViewModel>();

        // Views
        _ = services.AddTransient<ReportView>();
        _ = services.AddTransient<ReportDesignerView>();
        _ = services.AddTransient<ReportViewerModule>();
        _ = services.AddTransient<ReportDesignerModule>();
    }
}
