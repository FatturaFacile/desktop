// <copyright file="IAuthenticationService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Defines the contract for authenticating user credentials against Windows security authorities or local accounts.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user using Windows credentials.
    /// </summary>
    /// <param name="username">The account login username.</param>
    /// <param name="domain">The optional Windows domain name or machine name.</param>
    /// <param name="password">The plaintext password provided by the user for validation.</param>
    /// <returns>A task returning the <see cref="AuthenticationResult"/> indicating success or failure.</returns>
    Task<AuthenticationResult> AuthenticateWindowsUserAsync(string username, string? domain, string password);
}
