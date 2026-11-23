// <copyright file="SuisseTaxExportRow.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Wpf.ViewModels.SwissVat;

/// <summary>
/// Display row for a SuisseTax eCH-0217 XML export log entry.
/// </summary>
/// <param name="FileName">Exported file name.</param>
/// <param name="Sha256">SHA-256 digest.</param>
/// <param name="XsdStatus">XSD validation status.</param>
/// <param name="ExportedAt">Export timestamp.</param>
internal sealed record SuisseTaxExportRow(string FileName, string Sha256, string XsdStatus, string ExportedAt);
