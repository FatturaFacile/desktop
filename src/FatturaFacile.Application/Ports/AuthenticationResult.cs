// <copyright file="AuthenticationResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Encapsulates the outcome of an authentication attempt against local or domain Windows credentials.
/// </summary>
/// <param name="IsSuccess">Indicates whether credential verification succeeded.</param>
/// <param name="ErrorMessage">Descriptive error message when authentication fails; otherwise, null.</param>
/// <param name="Username">The authenticated account username.</param>
/// <param name="Domain">The optional Active Directory domain name of the authenticated user.</param>
public record AuthenticationResult(bool IsSuccess, string? ErrorMessage, string? Username, string? Domain);
