using System.Globalization;
using System.Xml.Linq;
using FatturaFacile.Domain.Entities.Purchasing;

namespace FatturaFacile.Infrastructure.CrossBorder.Iso20022;

/// <summary>
/// Represents iso20022 payment generator.
/// </summary>
public static class Iso20022PaymentGenerator
{
    private static readonly XNamespace Ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";

/// <summary>
/// Generate Pain001.
/// </summary>
/// <param name="instruction">The instruction.</param>
/// <returns>A task representing the result of the asynchronous operation, or the computed value.</returns>
    public static string GeneratePain001(Iso20022PaymentInstruction instruction)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", standalone: null),
            new XElement(
                Ns + "Document",
                new XElement(
                    Ns + "CstmrCdtTrfInitn",
                    new XElement(
                        Ns + "GrpHdr",
                        new XElement(Ns + "MsgId", instruction.InstructionId),
                        new XElement(Ns + "CreDtTm", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)),
                        new XElement(Ns + "NbOfTxs", "1"),
                        new XElement(
                            Ns + "InitgPty",
                            new XElement(Ns + "Nm", "FatturaFacile ERP Multi-Tenant"))),
                    new XElement(
                        Ns + "PmtInf",
                        new XElement(Ns + "PmtInfId", instruction.InstructionId),
                        new XElement(Ns + "PmtMtd", "TRF"),
                        new XElement(Ns + "ReqdExctnDt", instruction.ExecutionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                        new XElement(
                            Ns + "Dbtr",
                            new XElement(Ns + "Nm", "FatturaFacile Debtor")),
                        new XElement(
                            Ns + "DbtrAcct",
                            new XElement(
                                Ns + "Id",
                                new XElement(Ns + "IBAN", instruction.DebtorIban))),
                        new XElement(
                            Ns + "CdtTrfTxInf",
                            new XElement(
                                Ns + "PmtId",
                                new XElement(Ns + "EndToEndId", instruction.EndToEndId)),
                            new XElement(
                                Ns + "Amt",
                                new XElement(
                                    Ns + "InstdAmt",
                                    new XAttribute("Ccy", instruction.Amount.Currency.ToString()),
                                    instruction.Amount.Amount.ToString("0.00", CultureInfo.InvariantCulture))),
                            new XElement(
                                Ns + "Cdtr",
                                new XElement(Ns + "Nm", instruction.CreditorName)),
                            new XElement(
                                Ns + "CdtrAcct",
                                new XElement(
                                    Ns + "Id",
                                    new XElement(Ns + "IBAN", instruction.CreditorIban))),
                            new XElement(
                                Ns + "RmtInf",
                                new XElement(Ns + "Ustrd", instruction.RemittanceInformation)))))));

        return doc.ToString();
    }
}
