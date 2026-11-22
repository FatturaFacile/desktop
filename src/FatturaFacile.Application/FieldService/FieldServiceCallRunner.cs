// <copyright file="FieldServiceCallRunner.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Net;
using System.Text.Json;

namespace FatturaFacile.Application.FieldService;

/// <summary>
/// Turns the outcome of an online call (answer, status code or exception) into a <see cref="FieldServiceCallStatus"/>, so no
/// network failure ever escapes to the view models.
/// </summary>
public static class FieldServiceCallRunner
{
    /// <summary>
    /// Runs a call and classifies the failures: a missing status code means the server never answered (offline), an unreadable
    /// answer or a server error is a failure.
    /// </summary>
    /// <param name="call">The call.</param>
    /// <param name="cancellationToken">The cancellation token of the caller.</param>
    /// <returns>The result of the call, or the classified failure.</returns>
    public static async Task<FieldServiceCallResult> RunAsync(Func<Task<FieldServiceCallResult>> call, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(call);

        try
        {
            return await call().ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            return new FieldServiceCallResult(Classify(ex.StatusCode), ex.Message);
        }
        catch (JsonException ex)
        {
            return new FieldServiceCallResult(FieldServiceCallStatus.Failed, ex.Message);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // A timeout: the server did not answer in time.
            return new FieldServiceCallResult(FieldServiceCallStatus.Offline, ex.Message);
        }
    }

    /// <summary>
    /// Classifies the status code of a failed request; null means the request never got an answer.
    /// </summary>
    /// <param name="status">The status code, or null.</param>
    /// <returns>The call status.</returns>
    public static FieldServiceCallStatus Classify(HttpStatusCode? status) => status is { } code ? Classify(code) : FieldServiceCallStatus.Offline;

    /// <summary>
    /// Classifies the status code of an answer.
    /// </summary>
    /// <param name="status">The status code.</param>
    /// <returns>The call status.</returns>
    public static FieldServiceCallStatus Classify(HttpStatusCode status) => (int)status switch
    {
        >= 200 and < 300 => FieldServiceCallStatus.Ok,
        401 => FieldServiceCallStatus.Unauthorized,
        403 => FieldServiceCallStatus.Forbidden,
        408 or 429 => FieldServiceCallStatus.Offline,
        >= 400 and < 500 => FieldServiceCallStatus.Rejected,
        _ => FieldServiceCallStatus.Failed,
    };
}
