using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FatturaFacile.Infrastructure.Common.Diagnostics;

public static class FatturaFacileDiagnostics
{
    public const string ServiceName = "FatturaFacile";
    public static readonly ActivitySource ActivitySource = new(ServiceName, "1.0.0");
    public static readonly Meter Meter = new(ServiceName, "1.0.0");

    public static readonly Counter<long> InvoicesCreatedCounter = 
        Meter.CreateCounter<long>("fatturafacile_invoices_created_total", "count", "Total invoices created");
    
    public static readonly Counter<long> PaymentsProcessedCounter = 
        Meter.CreateCounter<long>("fatturafacile_payments_processed_total", "count", "Total payments processed");
}
