// <copyright file="BackupResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Ports;

/// <summary>
/// Encapsulates the execution status and output artifact location of a database backup or restore operation.
/// </summary>
/// <param name="Success">Indicates whether the backup or restore succeeded without errors.</param>
/// <param name="FilePath">The full local or network file path where the backup archive resides.</param>
/// <param name="ErrorMessage">Descriptive error message if the backup or restore failed; otherwise, null.</param>
public record BackupResult(bool Success, string FilePath, string? ErrorMessage);
