// <copyright file="RestoreBackupCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Backup.Commands;

/// <summary>
/// Restores a password-protected backup, overwriting database and key (system administrators only).
/// </summary>
public sealed class RestoreBackupCommand : IRequest<string>
{
    /// <summary>
    /// Gets or sets the backup zip path.
    /// </summary>
    public string ZipPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the zip password.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
