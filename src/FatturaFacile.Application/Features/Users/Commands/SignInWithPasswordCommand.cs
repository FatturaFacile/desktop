// <copyright file="SignInWithPasswordCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Signs in a user with user name and password (5 failed attempts lock the account for 15 minutes).
/// </summary>
/// <remarks>
/// A class rather than a record so that the password never appears in a compiler-generated <see cref="object.ToString"/>.
/// </remarks>
public sealed class SignInWithPasswordCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
