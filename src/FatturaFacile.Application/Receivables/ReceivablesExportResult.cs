// <copyright file="ReceivablesExportResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Receivables;

/// <summary>
/// The outcome of a statement export.
/// </summary>
/// <param name="Path">The file the statement was saved to, null when offline.</param>
/// <param name="Offline">Whether the server could not be reached.</param>
public sealed record ReceivablesExportResult(string? Path, bool Offline);
