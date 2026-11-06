// <copyright file="CamtStatementParser.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Banking;

/// <summary>
/// Parses ISO 20022 bank statements and notifications (camt.053 / camt.054, all versions used by Swiss and European banks):
/// one transaction per transaction detail, with amount sign, structured reference (QRR / SCOR), remittance text and
/// counterparty.
/// </summary>
public static class CamtStatementParser
{
    private static readonly string[] _containers = ["Stmt", "Ntfctn", "Rpt"];

    /// <summary>
    /// Parses a statement.
    /// </summary>
    /// <param name="xml">The camt XML.</param>
    /// <returns>The account IBAN of the statement and the transactions (without bank account).</returns>
    /// <exception cref="FormatException">When the document is not a camt.053/054 statement.</exception>
    public static (string? Iban, IReadOnlyList<BankTransaction> Transactions) Parse(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        XmlReaderSettings settings = new() { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
        using var stringReader = new StringReader(xml);
        using var reader = XmlReader.Create(stringReader, settings);
        var document = XDocument.Load(reader);

        var statements = document.Descendants().Where(static e => _containers.Contains(e.Name.LocalName, StringComparer.Ordinal)).ToList();

        if (statements.Count == 0)
        {
            throw new FormatException("Il file non è un estratto conto ISO 20022 (camt.053 / camt.054).");
        }

        string? iban = null;
        List<BankTransaction> transactions = [];

        foreach (var statement in statements)
        {
            iban ??= Value(Child(Child(Child(statement, "Acct"), "Id"), "IBAN"));

            foreach (var entry in statement.Elements().Where(static e => string.Equals(e.Name.LocalName, "Ntry", StringComparison.Ordinal)))
            {
                transactions.AddRange(ParseEntry(entry));
            }
        }

        return (iban, transactions);
    }

    private static List<BankTransaction> ParseEntry(XElement entry)
    {
        var sign = string.Equals(Value(Child(entry, "CdtDbtInd")), "DBIT", StringComparison.Ordinal) ? -1m : 1m;
        var entryAmount = Child(entry, "Amt");
        var booking = Date(Child(entry, "BookgDt")) ?? DateTime.Today;
        var value = Date(Child(entry, "ValDt"));
        var bankReference = Value(Child(entry, "AcctSvcrRef"));
        var details = entry.Descendants().Where(static e => string.Equals(e.Name.LocalName, "TxDtls", StringComparison.Ordinal)).ToList();
        List<BankTransaction> result = [];

        if (details.Count == 0)
        {
            result.Add(Create(entry, entryAmount, sign, booking, value, bankReference));
            return result;
        }

        for (var i = 0; i < details.Count; i++)
        {
            var amount = Child(details[i], "Amt") ?? Child(Child(Child(details[i], "AmtDtls"), "TxAmt"), "Amt") ?? entryAmount;
            var reference = details.Count == 1 ? bankReference : string.Create(CultureInfo.InvariantCulture, $"{bankReference}/{i + 1}");
            result.Add(Create(details[i], amount, sign, booking, value, reference));
        }

        return result;
    }

    private static BankTransaction Create(XElement source, XElement? amount, decimal sign, DateTime booking, DateTime? value, string? externalId)
    {
        var remittance = Child(source, "RmtInf");
        var structured = source.Descendants().FirstOrDefault(static e => string.Equals(e.Name.LocalName, "CdtrRefInf", StringComparison.Ordinal));
        var parties = Child(source, "RltdPties");
        var debtor = sign > 0m ? Child(parties, "Dbtr") : Child(parties, "Cdtr");
        var debtorAccount = sign > 0m ? Child(parties, "DbtrAcct") : Child(parties, "CdtrAcct");
        var name = Value(Child(debtor, "Nm")) ?? Value(Child(Child(debtor, "Pty"), "Nm"));
        var endToEnd = Value(Child(Child(source, "Refs"), "EndToEndId"));

        return new BankTransaction
        {
            BookingDate = booking,
            ValueDate = value,
            Amount = sign * decimal.Parse(amount?.Value ?? "0", NumberStyles.Number, CultureInfo.InvariantCulture),
            Currency = (string?)amount?.Attribute("Ccy") ?? "CHF",
            Counterparty = name,
            CounterpartyIban = Value(Child(Child(debtorAccount, "Id"), "IBAN")),
            Reference = Value(Child(structured, "Ref")) ?? (string.Equals(endToEnd, "NOTPROVIDED", StringComparison.Ordinal) ? null : endToEnd),
            Description = Remittance(remittance) ?? Value(Child(source, "AddtlNtryInf")) ?? Value(Child(source, "AddtlTxInf")),
            ExternalId = externalId,
        };
    }

    private static string? Remittance(XElement? information)
    {
        var lines = information?.Elements()
            .Where(static e => string.Equals(e.Name.LocalName, "Ustrd", StringComparison.Ordinal))
            .Select(static e => e.Value.Trim())
            .ToList() ?? [];

        return lines.Count == 0 ? null : string.Join(' ', lines);
    }

    private static XElement? Child(XElement? parent, string name) => parent?.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, name, StringComparison.Ordinal));

    private static string? Value(XElement? element) => element is null || string.IsNullOrWhiteSpace(element.Value) ? null : element.Value.Trim();

    private static DateTime? Date(XElement? element)
    {
        var text = Value(Child(element, "Dt")) ?? Value(Child(element, "DtTm"));
        return text is null ? null : DateTime.Parse(text[..Math.Min(10, text.Length)], CultureInfo.InvariantCulture, DateTimeStyles.None);
    }
}
