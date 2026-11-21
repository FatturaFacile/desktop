// <copyright file="AiCopilotCoordinator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.AiCopilot;
using FatturaFacile.Infrastructure.Sync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FatturaFacile.Infrastructure.AiCopilot;

/// <summary>
/// Runs the AI copilot operations one at a time in their own DI scope, for the panel and for the background sync alike.
/// </summary>
/// <param name="scopes">The scope factory (the service and the database context are scoped).</param>
/// <param name="options">The sync options (the AI surface lives on the same web-api).</param>
public sealed class AiCopilotCoordinator(IServiceScopeFactory scopes, IOptions<SyncOptions> options) : IAiCopilot, IDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);

    /// <inheritdoc/>
    public event EventHandler? Changed;

    /// <inheritdoc/>
    public bool IsConfigured => options.Value.IsEnabled;

    /// <inheritdoc/>
    public Task<AiCopilotSnapshot> LoadAsync(CancellationToken cancellationToken) =>
        RunAsync((service, token) => service.LoadAsync(token), cancellationToken);

    /// <inheritdoc/>
    public Task<AiRefreshResult> RefreshAsync(CancellationToken cancellationToken) =>
        RunAsync((service, token) => service.RefreshAsync(token), cancellationToken);

    /// <inheritdoc/>
    public Task<AiDecisionResult> DecideAsync(Guid id, AiDecisionRequest request, CancellationToken cancellationToken) =>
        RunAsync((service, token) => service.DecideAsync(id, request, token), cancellationToken);

    /// <summary>
    /// Pushes queued decisions and refreshes the cache for the background sync cycle, then notifies the panel.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the pass is done.</returns>
    public async Task RefreshInBackgroundAsync(CancellationToken cancellationToken)
    {
        _ = await RefreshAsync(cancellationToken).ConfigureAwait(false);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void Dispose() => gate.Dispose();

    private async Task<T> RunAsync<T>(Func<AiCopilotService, CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var scope = scopes.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                var service = scope.ServiceProvider.GetRequiredService<AiCopilotService>();
                return await operation(service, cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _ = gate.Release();
        }
    }
}
