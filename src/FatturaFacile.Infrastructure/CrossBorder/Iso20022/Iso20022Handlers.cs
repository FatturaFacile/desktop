using System.Globalization;
using System.Xml.Linq;
using FatturaFacile.Domain.Entities.Purchasing;

namespace FatturaFacile.Infrastructure.CrossBorder.Iso20022;

public static class Iso20022PaymentGenerator
{
    private static readonly XNamespace Ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";

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

public static class Camt053BankStatementParser
{
    public static (decimal OpeningBalance, decimal ClosingBalance, List<(DateTime Date, decimal Amount, string Remittance)> Entries) Parse(string xmlContent)
    {
        var entries = new List<(DateTime Date, decimal Amount, string Remittance)>();
        decimal opening = 0m;
        decimal closing = 0m;

        try
        {
            var doc = XDocument.Parse(xmlContent);
            var defaultNs = doc.Root?.Name.Namespace ?? XNamespace.None;

            var stmt = doc.Descendants(defaultNs + "Stmt").FirstOrDefault();
            if (stmt != null)
            {
                foreach (var bal in stmt.Elements(defaultNs + "Bal"))
                {
                    var cdOrPrtry = bal.Element(defaultNs + "Tp")?.Element(defaultNs + "CdOrPrtry")?.Element(defaultNs + "Cd")?.Value;
                    var amtStr = bal.Element(defaultNs + "Amt")?.Value;
                    if (decimal.TryParse(amtStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt))
                    {
                        if (string.Equals(cdOrPrtry, "OPBD", StringComparison.Ordinal))
                        {
                            opening = amt;
                        }
                        else if (string.Equals(cdOrPrtry, "CLBD", StringComparison.Ordinal))
                        {
                            closing = amt;
                        }
                    }
                }

                foreach (var ntry in stmt.Elements(defaultNs + "Ntry"))
                {
                    var amtStr = ntry.Element(defaultNs + "Amt")?.Value;
                    var cdtDbt = ntry.Element(defaultNs + "CdtDbtInd")?.Value;
                    var dtStr = ntry.Element(defaultNs + "BookgDt")?.Element(defaultNs + "Dt")?.Value;
                    var ustrd = ntry.Descendants(defaultNs + "Ustrd").FirstOrDefault()?.Value ?? string.Empty;

                    if (decimal.TryParse(amtStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var amt) &&
                        DateTime.TryParse(dtStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    {
                        var signedAmt = string.Equals(cdtDbt, "DBIT", StringComparison.Ordinal) ? -amt : amt;
                        entries.Add((dt, signedAmt, ustrd));
                    }
                }
            }
        }
        catch (System.Xml.XmlException)
        {
            // Fallback for mock XML
        }

        return (opening, closing, entries);
    }
}
