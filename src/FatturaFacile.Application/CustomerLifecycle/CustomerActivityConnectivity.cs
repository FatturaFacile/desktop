// <copyright file="CustomerActivityConnectivity.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;

namespace FatturaFacile.Application.CustomerLifecycle;

/// <summary>
/// Tells a server that is unreachable (worth retrying later) from one that refused the request (never retried).
/// </summary>
public static class CustomerActivityConnectivity
{
    /// <summary>
    /// Determines whether a failure means the server could not be reached.
    /// </summary>
    /// <param name="failure">The failure.</param>
    /// <param name="cancellationToken">The caller token; a cancellation requested through it is not a network failure.</param>
    /// <returns>True for network failures, timeouts and gateway errors; false for validation or authorization refusals.</returns>
    public static bool IsUnreachable(Exception failure, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(failure);

        return failure switch
        {
            HttpRequestException { StatusCode: null } => true,
            HttpRequestException { StatusCode: HttpStatusCode.RequestTimeout or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout } => true,
            TaskCanceledException => !cancellationToken.IsCancellationRequested,
            _ => false,
        };
    }
}
