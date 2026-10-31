// <copyright file="BackupInfo.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Provides metadata describing an existing database backup file.
/// </summary>
/// <param name="FileName">The file name of the backup archive.</param>
/// <param name="FullPath">The absolute file path on disk.</param>
/// <param name="SizeBytes">The backup file size in bytes.</param>
/// <param name="CreatedAt">The timestamp indicating when the backup file was generated.</param>
/// <param name="IsEncrypted">Indicates whether the backup file is secured with password-based encryption.</param>
public record BackupInfo(string FileName, string FullPath, long SizeBytes, DateTime CreatedAt, bool IsEncrypted);
