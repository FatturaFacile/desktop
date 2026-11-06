// <copyright file="IBackupService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Common.Interfaces;

/// <summary>
/// Creates and restores password-protected backups containing the database, the field encryption key and the other files
/// of the data folder (report templates, documents).
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Creates an AES-256 password-protected zip with a consistent copy of the database and the encryption key.
    /// </summary>
    /// <param name="zipPath">The destination zip path.</param>
    /// <param name="password">The zip password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the backup.</returns>
    public Task CreateBackupAsync(string zipPath, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a backup, overwriting the database and the encryption key (the current database is kept as a safety copy).
    /// The application must be restarted afterwards.
    /// </summary>
    /// <param name="zipPath">The backup zip path.</param>
    /// <param name="password">The zip password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The path of the safety copy of the replaced database.</returns>
    public Task<string> RestoreBackupAsync(string zipPath, string password, CancellationToken cancellationToken = default);
}
