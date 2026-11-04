// <copyright file="FatturaFacileDiagnostics.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FatturaFacile.Infrastructure.Common.Diagnostics;

/// <summary>
/// Provides OpenTelemetry diagnostic instrumentation, ActivitySource instances, and application metric counters.
/// </summary>
public static class FatturaFacileDiagnostics
{
    /// <summary>
    /// The telemetry service and meter name.
    /// </summary>
    public const string ServiceName = "FatturaFacile";

    /// <summary>
    /// Activity source for distributed trace spans across ERP operations.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new(ServiceName, "1.0.0");

    /// <summary>
    /// Application meter for recording business performance metrics and counters.
    /// </summary>
    public static readonly Meter Meter = new(ServiceName, "1.0.0");

    /// <summary>
    /// Metric counter recording total invoices generated across tenants.
    /// </summary>
    public static readonly Counter<long> InvoicesCreatedCounter = Meter.CreateCounter<long>(
        "fatturafacile_invoices_created_total",
        "count",
        "Total invoices created");

    /// <summary>
    /// Metric counter recording total invoice payments settled.
    /// </summary>
    public static readonly Counter<long> PaymentsProcessedCounter = Meter.CreateCounter<long>(
        "fatturafacile_payments_processed_total",
        "count",
        "Total payments processed");
}
