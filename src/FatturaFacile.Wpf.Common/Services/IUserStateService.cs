// <copyright file="IUserStateService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.Common.Services;

public sealed record UserState(string LogicalState = "", string VisualState = "");

public interface IUserStateService
{
    ValueTask<UserState> LoadStateAsync(CancellationToken cancellationToken = default);
    ValueTask SaveStateAsync(UserState state, CancellationToken cancellationToken = default);
    ValueTask<UserState> LoadStateAsync(CancellationToken cancellationToken = default);
    ValueTask SaveStateAsync(UserState state, CancellationToken cancellationToken = default);
    ValueTask ResetStateAsync(CancellationToken cancellationToken = default);
}
