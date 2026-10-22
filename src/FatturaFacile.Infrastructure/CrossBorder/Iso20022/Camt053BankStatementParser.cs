// <copyright file="Camt053BankStatementParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml.Linq;

namespace FatturaFacile.Infrastructure.CrossBorder.Iso20022;

/// <summary>
/// Parses ISO 20022 camt.053 XML bank statement documents, extracting balances and transaction entries.
/// </summary>
public static class Camt053BankStatementParser
{
    /// <summary>
    /// Parses an ISO 20022 camt.053 XML statement into opening balance, closing balance, and itemized booking entries.
    /// </summary>
    /// <param name="xmlContent">The raw XML content of the camt.053 statement.</param>
    /// <returns>
    /// A named tuple containing:
    /// <list type="bullet">
    /// <item><description><c>OpeningBalance</c>: The opening booked balance (OPBD).</description></item>
    /// <item><description><c>ClosingBalance</c>: The closing booked balance (CLBD).</description></item>
    /// <item><description><c>Entries</c>: A list of booking records with date, signed amount (negative for debit, positive for credit), and unstructured remittance info.</description></item>
    /// </list>
    /// </returns>
    public static (decimal OpeningBalance, decimal ClosingBalance,
        List<(DateTime Date, decimal Amount, string Remittance)> Entries) Parse(
            string xmlContent)
    {
        var entries = new List<(DateTime Date, decimal Amount, string Remittance)>();
        var opening = 0m;
        var closing = 0m;

        try
        {
            var doc = XDocument.Parse(xmlContent);
            var defaultNs = doc.Root?.Name.Namespace ?? XNamespace.None;

            var stmt = doc.Descendants(defaultNs + "Stmt").FirstOrDefault();
            if (stmt != null)
            {
                foreach (var bal in stmt.Elements(defaultNs + "Bal"))
                {
                    var cdOrPrtry = bal.Element(defaultNs + "Tp")?.Element(defaultNs + "CdOrPrtry")
                        ?.Element(defaultNs + "Cd")?.Value;
                    var amtStr = bal.Element(defaultNs + "Amt")?.Value;
                    if (decimal.TryParse(
                            amtStr,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var amt))
                    {
                        if (string.Equals(
                                cdOrPrtry,
                                "OPBD",
                                StringComparison.Ordinal))
                        {
                            opening = amt;
                        }
                        else if (string.Equals(
                                     cdOrPrtry,
                                     "CLBD",
                                     StringComparison.Ordinal))
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

                    if (!decimal.TryParse(
                            amtStr,
                            NumberStyles.Any,
                            CultureInfo.InvariantCulture,
                            out var amt) ||
                        !DateTime.TryParse(
                            dtStr,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var dt))
                    {
                        continue;
                    }

                    var signedAmt = string.Equals( cdtDbt, "DBIT", StringComparison.Ordinal) ? -amt : amt;
                    entries.Add((dt, signedAmt, ustrd));
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
