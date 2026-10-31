// <copyright file="TenantBackup.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Common;

namespace FatturaFacile.Domain.Entities;

/// <summary>
/// Records the audit trail and file metadata of a completed database backup archive for a tenant.
/// </summary>
public class TenantBackup : EntityBase, IMustHaveTenant
{
    /// <inheritdoc />
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the file name of the generated backup archive.
    /// </summary>
    public string BackupFileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute path on disk or network storage where the backup file is preserved.
    /// </summary>
    public string FullFilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the backup archive file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the archive is protected with AES-256 password encryption.
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Gets or sets the cryptographic SHA-256 hash checksum of the backup archive file.
    /// </summary>
    public string? ChecksumSha256 { get; set; }
}
