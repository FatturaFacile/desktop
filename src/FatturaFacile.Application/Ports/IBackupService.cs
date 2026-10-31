// <copyright file="IBackupService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Provides backup creation, automated scheduling, password encryption, and disaster recovery restore operations for tenant databases.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Creates an immediate online SQLite database backup snapshot to the specified destination path.
    /// </summary>
    /// <param name="destinationPath">The file path where the snapshot should be saved.</param>
    /// <param name="cancellationToken">Cancellation token to observe during the backup process.</param>
    /// <returns>A task returning the backup execution result.</returns>
    Task<BackupResult> CreateOnlineSnapshotAsync(string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an automated silent backup according to tenant retention and directory configuration.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to observe during the backup process.</param>
    /// <returns>A task returning the automated backup execution result.</returns>
    Task<BackupResult> PerformAutomatedSilentBackupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a database backup secured with AES password-based encryption.
    /// </summary>
    /// <param name="targetFilePath">The output encrypted backup file path.</param>
    /// <param name="password">The encryption password.</param>
    /// <param name="cancellationToken">Cancellation token to observe during export.</param>
    /// <returns>A task returning the backup export result.</returns>
    Task<BackupResult> ExportEncryptedBackupAsync(string targetFilePath, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores a tenant database from a backup file, optionally decrypting it with a password.
    /// </summary>
    /// <param name="sourceFilePath">The path of the backup archive to restore from.</param>
    /// <param name="password">The decryption password if the file was encrypted; otherwise, null.</param>
    /// <param name="cancellationToken">Cancellation token to observe during restore.</param>
    /// <returns>A task returning the restore execution result.</returns>
    Task<BackupResult> RestoreBackupAsync(string sourceFilePath, string? password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all recognized backup archives in the configured backup directory.
    /// </summary>
    /// <returns>A read-only collection of <see cref="BackupInfo"/> metadata records.</returns>
    IReadOnlyList<BackupInfo> GetAvailableBackups();
}
