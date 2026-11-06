// <copyright file="PurchaseImportResult.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.Purchasing;

/// <summary>
/// Outcome of a supplier invoice import.
/// </summary>
/// <param name="Created">The new supplier invoices (drafts to check and post).</param>
/// <param name="Skipped">The invoices already present.</param>
public sealed record PurchaseImportResult(int Created, int Skipped);
