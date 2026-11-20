// <copyright file="SyncBackgroundService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// Triggers a sync pass periodically while the application runs.
/// </summary>
/// <param name="coordinator">The sync coordinator.</param>
/// <param name="options">The sync options.</param>
/// <param name="logger">The logger.</param>
public sealed partial class SyncBackgroundService(SyncCoordinator coordinator, IOptions<SyncOptions> options, ILogger<SyncBackgroundService> logger) : BackgroundService
{
    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.IsEnabled)
        {
            return;
        }

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(Math.Max(15, options.Value.IntervalSeconds)));
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                _ = await coordinator.RunNowAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                // A failed pass must never stop the timer: the next tick retries.
                LogPassFailed(logger, ex);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sync pass failed.")]
    private static partial void LogPassFailed(ILogger logger, Exception exception);
}
