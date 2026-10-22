// <copyright file="IUserStateService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Common.Services;

/// <summary>
/// Service contract for loading, persisting, and resetting user state.
/// </summary>
public interface IUserStateService
{
    ValueTask<UserState> LoadStateAsync(
        CancellationToken cancellationToken = default);

    ValueTask SaveStateAsync(
        UserState state,
        CancellationToken cancellationToken = default);

    ValueTask ResetStateAsync(
        CancellationToken cancellationToken = default);
}
