// <copyright file="SyncCoordinator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using FatturaFacile.Application.Security;
using FatturaFacile.Application.Sync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FatturaFacile.Infrastructure.Sync;

/// <summary>
/// Runs sync passes one at a time, for the background timer and for the "Sync Now" command alike.
/// </summary>
/// <param name="scopes">The scope factory (the engine is scoped).</param>
/// <param name="options">The sync options.</param>
public sealed class SyncCoordinator(IServiceScopeFactory scopes, IOptions<SyncOptions> options) : IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <summary>
    /// Occurs after every completed pass.
    /// </summary>
    public event EventHandler? Completed;

    /// <summary>
    /// Gets the result of the last pass, or null before the first one.
    /// </summary>
    public SyncRunResult? LastResult { get; private set; }

    /// <summary>
    /// Runs a pass now; concurrent calls wait for the running one.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary of the pass.</returns>
    public async Task<SyncRunResult> RunNowAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.IsEnabled)
        {
            return SyncRunResult.Disabled;
        }

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var scope = scopes.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                var engine = scope.ServiceProvider.GetRequiredService<SyncEngine>();
                var result = await engine.RunAsync(cancellationToken).ConfigureAwait(false);
                LastResult = result.AuthRequired
                    ? result
                    : await AddAuditPassAsync(scope.ServiceProvider, result, cancellationToken).ConfigureAwait(false);
            }

            Completed?.Invoke(this, EventArgs.Empty);
            return LastResult;
        }
        finally
        {
            _ = gate.Release();
        }
    }

    /// <inheritdoc/>
    public void Dispose() => gate.Dispose();

    private static async Task<SyncRunResult> AddAuditPassAsync(IServiceProvider services, SyncRunResult result, CancellationToken cancellationToken)
    {
        var synchronizer = services.GetService<AuditQueueSynchronizer>();
        if (synchronizer is null)
        {
            return result;
        }

        try
        {
            var audit = await synchronizer.RunAsync(cancellationToken).ConfigureAwait(false);
            return audit switch
            {
                { ChainBroken: true } => result with { Errors = [.. result.Errors, $"Audit queue not uploaded: chain verification failed ({audit.Error})."] },
                { AuthRequired: true } => result with { AuthRequired = true },
                { Error: not null } => result with { Errors = [.. result.Errors, $"Audit queue: {audit.Error}"] },
                _ => result,
            };
        }
        catch (CryptographicException ex)
        {
            return result with { Errors = [.. result.Errors, $"Audit queue key unavailable: {ex.Message}"] };
        }
    }

}
