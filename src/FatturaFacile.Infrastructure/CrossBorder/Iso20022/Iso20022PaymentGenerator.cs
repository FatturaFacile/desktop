// <copyright file="Iso20022PaymentGenerator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;

using FatturaFacile.Domain.Entities.Purchasing;

namespace FatturaFacile.Infrastructure.CrossBorder.Iso20022;

/// <summary>
/// Generates ISO 20022 Customer Credit Transfer Initiation messages (pain.001.001.03) for SEPA/cross-border payment orders.
/// </summary>
public static class Iso20022PaymentGenerator
{

    /// <summary>
    /// The XML namespace for pain.001.001.03 schema.
    /// </summary>
    private static readonly XNamespace Ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";

    /// <summary>
    /// Generates a valid pain.001.001.03 XML document for the specified payment instruction.
    /// </summary>
    /// <param name="instruction">The payment instruction containing debtor, creditor, amount, and remittance details.</param>
    /// <returns>A string containing the serialized UTF-8 XML document representing the credit transfer initiation.</returns>
    public static string GeneratePain001(Iso20022PaymentInstruction instruction)
    {
        var doc = new XDocument(
                                declaration: new XDeclaration(
                                                              version: "1.0",
                                                              encoding: "UTF-8",
                                                              standalone: null),
                                content: new XElement(
                                                      name: Ns + "Document",
                                                      content: new XElement(
                                                                            name: Ns + "CstmrCdtTrfInitn",
                                                                            content:
                                                                            [
                                                                                new XElement(
                                                                                             name: Ns + "GrpHdr",
                                                                                             content:
                                                                                             [
                                                                                                 new XElement(
                                                                                                              name: Ns + "MsgId",
                                                                                                              content: instruction.InstructionId),
                                                                                                 new XElement(
                                                                                                              name: Ns + "CreDtTm",
                                                                                                              content: DateTime.UtcNow.ToString(
                                                                                                                                                format: "yyyy-MM-ddTHH:mm:ssZ",
                                                                                                                                                provider: CultureInfo
                                                                                                                                                   .InvariantCulture)),
                                                                                                 new XElement(
                                                                                                              name: Ns + "NbOfTxs",
                                                                                                              content: "1"),
                                                                                                 new XElement(
                                                                                                              name: Ns + "InitgPty",
                                                                                                              content: new XElement(
                                                                                                                                    name: Ns + "Nm",
                                                                                                                                    content: "FatturaFacile ERP Multi-Tenant"))
                                                                                             ]),
                                                                                new XElement(
                                                                                             name: Ns + "PmtInf",
                                                                                             content:
                                                                                             [
                                                                                                 new XElement(
                                                                                                              name: Ns + "PmtInfId",
                                                                                                              content: instruction.InstructionId),
                                                                                                 new XElement(
                                                                                                              name: Ns + "PmtMtd",
                                                                                                              content: "TRF"),
                                                                                                 new XElement(
                                                                                                              name: Ns + "ReqdExctnDt",
                                                                                                              content: instruction.ExecutionDate.ToString(
                                                                                                                                                          format: "yyyy-MM-dd",
                                                                                                                                                          provider: CultureInfo
                                                                                                                                                             .InvariantCulture)),
                                                                                                 new XElement(
                                                                                                              name: Ns + "Dbtr",
                                                                                                              content: new XElement(
                                                                                                                                    name: Ns + "Nm",
                                                                                                                                    content: "FatturaFacile Debtor")),
                                                                                                 new XElement(
                                                                                                              name: Ns + "DbtrAcct",
                                                                                                              content: new XElement(
                                                                                                                                    name: Ns + "Id",
                                                                                                                                    content: new XElement(
                                                                                                                                                          name: Ns +
                                                                                                                                                                "IBAN",
                                                                                                                                                          content: instruction
                                                                                                                                                             .DebtorIban))),
                                                                                                 new XElement(
                                                                                                              name: Ns + "CdtTrfTxInf",
                                                                                                              content:
                                                                                                              [
                                                                                                                  new XElement(
                                                                                                                               name: Ns + "PmtId",
                                                                                                                               content: new XElement(
                                                                                                                                                     name: Ns +
                                                                                                                                                           "EndToEndId",
                                                                                                                                                     content: instruction
                                                                                                                                                        .EndToEndId)),
                                                                                                                  new XElement(
                                                                                                                               name: Ns + "Amt",
                                                                                                                               content: new XElement(
                                                                                                                                                     name: Ns +
                                                                                                                                                           "InstdAmt",
                                                                                                                                                     content:
                                                                                                                                                     [
                                                                                                                                                         new
                                                                                                                                                             XAttribute(
                                                                                                                                                                        name: "Ccy",
                                                                                                                                                                        value: instruction
                                                                                                                                                                              .Amount
                                                                                                                                                                              .Currency
                                                                                                                                                                              .ToString()),
                                                                                                                                                         instruction
                                                                                                                                                            .Amount
                                                                                                                                                            .Amount
                                                                                                                                                            .ToString(
                                                                                                                                                                      format: "0.00",
                                                                                                                                                                      provider: CultureInfo
                                                                                                                                                                         .InvariantCulture)
                                                                                                                                                     ])),
                                                                                                                  new XElement(
                                                                                                                               name: Ns + "Cdtr",
                                                                                                                               content: new XElement(
                                                                                                                                                     name: Ns + "Nm",
                                                                                                                                                     content: instruction
                                                                                                                                                        .CreditorName)),
                                                                                                                  new XElement(
                                                                                                                               name: Ns + "CdtrAcct",
                                                                                                                               content: new XElement(
                                                                                                                                                     name: Ns + "Id",
                                                                                                                                                     content: new
                                                                                                                                                         XElement(
                                                                                                                                                                  name: Ns +
                                                                                                                                                                        "IBAN",
                                                                                                                                                                  content: instruction
                                                                                                                                                                     .CreditorIban))),
                                                                                                                  new XElement(
                                                                                                                               name: Ns + "RmtInf",
                                                                                                                               content: new XElement(
                                                                                                                                                     name: Ns +
                                                                                                                                                           "Ustrd",
                                                                                                                                                     content: instruction
                                                                                                                                                        .RemittanceInformation))
                                                                                                              ])
                                                                                             ])
                                                                            ])));

        return doc.ToString();
    }

}
