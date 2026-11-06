// <copyright file="SignInWindowsUserCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Users.Commands;

/// <summary>
/// Signs in the Windows account verified by Windows Hello. On a fresh installation (no users) the account
/// becomes the first system administrator.
/// </summary>
/// <param name="WindowsAccountName">The Windows account (DOMAIN\user).</param>
/// <param name="DisplayName">The optional display name for the bootstrap administrator.</param>
public sealed record SignInWindowsUserCommand(string WindowsAccountName, string? DisplayName = null) : IRequest<bool>;
