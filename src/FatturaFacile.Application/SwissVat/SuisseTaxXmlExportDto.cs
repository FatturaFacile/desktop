// <copyright file="SuisseTaxXmlExportDto.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

namespace FatturaFacile.Application.SwissVat;

/// <summary>
/// Data transfer object for a persisted SuisseTax eCH-0217 XML export.
/// </summary>
/// <param name="Id">Export identifier.</param>
/// <param name="SwissVatReturnId">Associated return identifier.</param>
/// <param name="FileName">Exported XML file name.</param>
/// <param name="Sha256Checksum">SHA-256 hex digest of XML payload.</param>
/// <param name="XsdValid">True when XSD validation passed.</param>
/// <param name="QuadraturaPassed">True when quadratura passed.</param>
/// <param name="XmlPayload">Generated XML payload.</param>
/// <param name="ExportedAt">UTC export timestamp.</param>
public sealed record SuisseTaxXmlExportDto(
    Guid Id,
    Guid SwissVatReturnId,
    string FileName,
    string Sha256Checksum,
    bool XsdValid,
    bool QuadraturaPassed,
    string XmlPayload,
    DateTime ExportedAt);
