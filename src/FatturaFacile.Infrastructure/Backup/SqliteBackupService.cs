// <copyright file="SqliteBackupService.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using FatturaFacile.Application.Ports;
using FatturaFacile.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FatturaFacile.Infrastructure.Backup;

/// <summary>
/// Provides SQLite database backup management, automated compressed snapshot rotation,
/// AES-256 GCM authenticated encryption/export, and disaster recovery restore routines.
/// </summary>
public class SqliteBackupService : IBackupService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SqliteBackupService> _logger;
    private readonly string _databaseFilePath;
    private readonly string _backupDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteBackupService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger instance.</param>
    public SqliteBackupService(AppDbContext context, ILogger<SqliteBackupService> logger)
    {
        _context = context;
        _logger = logger;

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(localAppData, "FatturaFacile");
        _ = Directory.CreateDirectory(appFolder);

        _backupDirectory = Path.Combine(appFolder, "Backups");
        _ = Directory.CreateDirectory(_backupDirectory);

        _databaseFilePath = Path.Combine(appFolder, "fatturafacile.db");
    }

    /// <inheritdoc />
    public async Task<BackupResult> CreateOnlineSnapshotAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var sourceConnString = _context.Database.GetConnectionString();
            if (string.IsNullOrEmpty(sourceConnString))
            {
                sourceConnString = $"Data Source={_databaseFilePath}";
            }

            var destConnString = $"Data Source={destinationPath}";

            await using var sourceConn = new SqliteConnection(sourceConnString);
            await using var destConn = new SqliteConnection(destConnString);

            await sourceConn.OpenAsync(cancellationToken);
            await destConn.OpenAsync(cancellationToken);

            // Native SQLite Online Backup API
            sourceConn.BackupDatabase(destConn);

            return new BackupResult(true, destinationPath, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create SQLite online snapshot at {Path}", destinationPath);
            return new BackupResult(false, destinationPath, ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<BackupResult> PerformAutomatedSilentBackupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var tempSnapshot = Path.Combine(_backupDirectory, $"temp_{timestamp}.db");
            var zipPath = Path.Combine(_backupDirectory, $"backup_auto_{timestamp}.zip");

            var snapshotResult = await CreateOnlineSnapshotAsync(tempSnapshot, cancellationToken);
            if (!snapshotResult.Success)
            {
                return snapshotResult;
            }

            // Compress to ZIP archive
            using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                _ = archive.CreateEntryFromFile(tempSnapshot, "database.db", CompressionLevel.Optimal);
            }

            // Clean up temporary snapshot file
            if (File.Exists(tempSnapshot))
            {
                File.Delete(tempSnapshot);
            }

            // Apply retention rotation: auto-delete backups older than 30 days or exceeding 20 archive count
            EnforceRetentionPolicy();

            return new BackupResult(true, zipPath, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Silent automated backup failed.");
            return new BackupResult(false, string.Empty, ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<BackupResult> ExportEncryptedBackupAsync(string targetFilePath, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is required for encrypted export.", nameof(password));
            }

            var tempSnapshot = Path.GetTempFileName();
            var snapshotResult = await CreateOnlineSnapshotAsync(tempSnapshot, cancellationToken);
            if (!snapshotResult.Success)
            {
                return snapshotResult;
            }

            var dbBytes = await File.ReadAllBytesAsync(tempSnapshot, cancellationToken);
            File.Delete(tempSnapshot);

            // AES-256 GCM authenticated encryption with PBKDF2 key derivation
            var salt = RandomNumberGenerator.GetBytes(16);
            var nonce = RandomNumberGenerator.GetBytes(12); // 96-bit nonce for AES-GCM
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);

            var ciphertext = new byte[dbBytes.Length];
            var tag = new byte[16]; // 128-bit authentication tag

            using (var aesGcm = new AesGcm(key, 16))
            {
                aesGcm.Encrypt(nonce, dbBytes, ciphertext, tag);
            }

            // File format: [Magic 4B: FFBC] [Salt 16B] [Nonce 12B] [Tag 16B] [Ciphertext]
            await using (var fs = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var magic = Encoding.ASCII.GetBytes("FFBC"); // Fattura Facile Backup Cipher
                await fs.WriteAsync(magic, cancellationToken);
                await fs.WriteAsync(salt, cancellationToken);
                await fs.WriteAsync(nonce, cancellationToken);
                await fs.WriteAsync(tag, cancellationToken);
                await fs.WriteAsync(ciphertext, cancellationToken);
            }

            return new BackupResult(true, targetFilePath, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export encrypted backup to {Path}", targetFilePath);
            return new BackupResult(false, targetFilePath, ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<BackupResult> RestoreBackupAsync(string sourceFilePath, string? password, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(sourceFilePath))
            {
                return new BackupResult(false, sourceFilePath, "Backup file does not exist.");
            }

            byte[] restoredDbBytes;

            // Check if file is encrypted (FFBC magic header)
            var fileBytes = await File.ReadAllBytesAsync(sourceFilePath, cancellationToken);
            if (fileBytes.Length > 48 && Encoding.ASCII.GetString(fileBytes, 0, 4) == "FFBC")
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    return new BackupResult(false, sourceFilePath, "This backup is password-protected. Please provide a password.");
                }

                var salt = new byte[16];
                var nonce = new byte[12];
                var tag = new byte[16];
                var cipherLen = fileBytes.Length - 4 - 16 - 12 - 16;
                var ciphertext = new byte[cipherLen];

                Buffer.BlockCopy(fileBytes, 4, salt, 0, 16);
                Buffer.BlockCopy(fileBytes, 20, nonce, 0, 12);
                Buffer.BlockCopy(fileBytes, 32, tag, 0, 16);
                Buffer.BlockCopy(fileBytes, 48, ciphertext, 0, cipherLen);

                var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
                restoredDbBytes = new byte[cipherLen];

                try
                {
                    using var aesGcm = new AesGcm(key, 16);
                    aesGcm.Decrypt(nonce, ciphertext, tag, restoredDbBytes);
                }
                catch (CryptographicException)
                {
                    return new BackupResult(false, sourceFilePath, "Invalid password or corrupted backup file.");
                }
            }
            // If ZIP archive
            else if (sourceFilePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                using var zip = ZipFile.OpenRead(sourceFilePath);
                var entry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".db", StringComparison.OrdinalIgnoreCase)) ?? zip.Entries.FirstOrDefault();
                if (entry == null)
                {
                    return new BackupResult(false, sourceFilePath, "No database file found inside the ZIP archive.");
                }

                await using var entryStream = entry.Open();
                using var ms = new MemoryStream();
                await entryStream.CopyToAsync(ms, cancellationToken);
                restoredDbBytes = ms.ToArray();
            }
            else
            {
                restoredDbBytes = fileBytes;
            }

            // Close existing connections and reset connection pool
            SqliteConnection.ClearAllPools();

            // Backup existing DB just in case
            if (File.Exists(_databaseFilePath))
            {
                var preRestoreBackup = _databaseFilePath + ".prerestore";
                File.Copy(_databaseFilePath, preRestoreBackup, true);
            }

            await File.WriteAllBytesAsync(_databaseFilePath, restoredDbBytes, cancellationToken);

            return new BackupResult(true, _databaseFilePath, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore backup from {Path}", sourceFilePath);
            return new BackupResult(false, sourceFilePath, ex.Message);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<BackupInfo> GetAvailableBackups()
    {
        if (!Directory.Exists(_backupDirectory))
        {
            return Array.Empty<BackupInfo>();
        }

        var list = new List<BackupInfo>();
        var files = Directory.GetFiles(_backupDirectory, "*.*", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is not (".zip" or ".enc" or ".ffb" or ".db"))
            {
                continue;
            }

            var fi = new FileInfo(file);
            var isEncrypted = false;
            try
            {
                using var fs = File.OpenRead(file);
                var header = new byte[4];
                if (fs.Read(header, 0, 4) == 4 && Encoding.ASCII.GetString(header) == "FFBC")
                {
                    isEncrypted = true;
                }
            }
            catch
            {
                // Fall back gracefully on read error
            }

            list.Add(new BackupInfo(fi.Name, fi.FullName, fi.Length, fi.CreationTimeUtc, isEncrypted));
        }

        return list.OrderByDescending(b => b.CreatedAt).ToList();
    }

    private void EnforceRetentionPolicy()
    {
        try
        {
            var backups = Directory.GetFiles(_backupDirectory, "backup_auto_*.zip")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTimeUtc)
                .ToList();

            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            // Delete backups older than 30 days
            foreach (var file in backups.Where(f => f.CreationTimeUtc < cutoffDate))
            {
                try { file.Delete(); } catch { }
            }

            // Re-fetch and enforce max 20 backups limit
            var remaining = Directory.GetFiles(_backupDirectory, "backup_auto_*.zip")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTimeUtc)
                .Skip(20)
                .ToList();

            foreach (var file in remaining)
            {
                try { file.Delete(); } catch { }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute backup retention policy cleanup.");
        }
    }
}
