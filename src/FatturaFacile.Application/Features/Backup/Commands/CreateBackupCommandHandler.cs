// <copyright file="CreateBackupCommandHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using MediatR;

namespace FatturaFacile.Application.Features.Backup.Commands;

/// <summary>
/// Handler for <see cref="CreateBackupCommand"/>.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="CreateBackupCommandHandler"/> class.
/// </remarks>
/// <param name="backupService">The backup service.</param>
/// <param name="currentUserService">The current user service.</param>
public sealed class CreateBackupCommandHandler(IBackupService backupService, ICurrentUserService currentUserService)
    : IRequestHandler<CreateBackupCommand, Unit>
{
    /// <inheritdoc/>
    public Task<Unit> Handle(CreateBackupCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return currentUserService.User?.IsSystemAdministrator != true
            ? throw new UnauthorizedAccessException("Solo un amministratore di sistema può eseguire il backup.")
            : HandleInternalAsync(request, cancellationToken);
    }

    private async Task<Unit> HandleInternalAsync(CreateBackupCommand request, CancellationToken cancellationToken)
    {
        await backupService.CreateBackupAsync(request.ZipPath, request.Password, cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
