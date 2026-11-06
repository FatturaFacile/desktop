// <copyright file="RestoreBackupCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using MediatR;

namespace FatturaFacile.Application.Features.Backup.Commands;

/// <summary>
/// Handler for <see cref="RestoreBackupCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RestoreBackupCommandHandler"/> class.
/// </remarks>
/// <param name="backupService">The backup service.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class RestoreBackupCommandHandler(IBackupService backupService, ICurrentUserService currentUserService)
    : IRequestHandler<RestoreBackupCommand, string>
{
    /// <inheritdoc/>
    public Task<string> Handle(RestoreBackupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return currentUserService.User?.IsSystemAdministrator != true
            ? throw new UnauthorizedAccessException("Solo un amministratore di sistema può ripristinare un backup.")
            : backupService.RestoreBackupAsync(request.ZipPath, request.Password, cancellationToken);
    }
}
