// <copyright file="IUserStateService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Common.Services;

/// <summary>
/// Service contract for loading, persisting, and resetting visual layout and logical state for the user session.
/// </summary>
public interface IUserStateService
{

    /// <summary>
    /// Loads the stored user layout and navigation state from persistent storage asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A value task containing the restored <see cref="UserState"/>, or a default empty state if none exists or loading fails.</returns>
    ValueTask<UserState> LoadStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the specified user layout and navigation state to storage asynchronously.
    /// </summary>
    /// <param name="state">The user state snapshot to persist.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A value task representing the asynchronous save operation.</returns>
    ValueTask SaveStateAsync(
        UserState         state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets persisted user state by removing saved storage files asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A value task representing the asynchronous reset operation.</returns>
    ValueTask ResetStateAsync(CancellationToken cancellationToken = default);

}
