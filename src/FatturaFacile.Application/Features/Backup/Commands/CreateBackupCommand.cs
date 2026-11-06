// <copyright file="CreateBackupCommand.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using MediatR;

namespace FatturaFacile.Application.Features.Backup.Commands;

/// <summary>
/// Creates a password-protected backup (system administrators only, since it contains all companies).
/// </summary>
public sealed class CreateBackupCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or sets the destination zip path.
    /// </summary>
    public string ZipPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the zip password (min. 8 characters).
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
