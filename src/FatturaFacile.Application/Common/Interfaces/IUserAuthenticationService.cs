// <copyright file="IUserAuthenticationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Provides biometric, Windows Hello, and hardware-backed user verification at application launch.
/// </summary>
public interface IUserAuthenticationService
{
    /// <summary>
    /// Authenticates the currently active Windows user via Windows Hello, biometric sensor, or PIN/Passkey.
    /// </summary>
    /// <param name="promptMessage">The prompt message displayed to the user.</param>
    /// <returns>True if authentication succeeded; false if canceled or denied.</returns>
    public Task<bool> AuthenticateCurrentUserAsync(string promptMessage);
}
