// <copyright file="CreateFirstAdministratorCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Creates the first system administrator on a fresh installation and signs it in. Rejected once any user exists.
/// </summary>
public sealed class CreateFirstAdministratorCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the login name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password (min. 12 characters).
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
