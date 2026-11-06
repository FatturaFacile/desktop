// <copyright file="BankImportResultDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.DTOs;

/// <summary>
/// The result of a bank statement import.
/// </summary>
/// <param name="Imported">The imported movements.</param>
/// <param name="Duplicates">The movements already imported.</param>
/// <param name="Matched">The movements reconciled automatically.</param>
public sealed record BankImportResultDto(int Imported, int Duplicates, int Matched);
