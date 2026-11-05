// <copyright file="SwissInvoicingStrategy.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Text;
using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Swiss invoicing strategy producing ISO 20022 compliant Swiss QR-Bill data.
/// </summary>
public sealed class SwissInvoicingStrategy : ICountryInvoicingStrategy
{
    /// <inheritdoc/>
    public string CountryCode => "CH";

    /// <summary>
    /// Generates a standard 27-digit Structured Creditor Reference (SCOR) with Modulo 10 recursive check digit.
    /// </summary>
    /// <param name="invoiceNumber">The invoice number string.</param>
    /// <returns>A 27-digit SCOR string.</returns>
    public static string GenerateScorReference(string invoiceNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invoiceNumber);

        // Extract numeric digits from invoice number, pad to 26 digits, and calculate mod 10 check digit
        StringBuilder digitsOnly = new();

        foreach (var c in invoiceNumber)
        {
            if (char.IsAsciiDigit(c))
            {
                _ = digitsOnly.Append(c);
            }
        }

        var rawNum = digitsOnly.Length > 0 ? digitsOnly.ToString() : "1";

        if (rawNum.Length > 26)
        {
            rawNum = rawNum[..26];
        }

        var padded = rawNum.PadLeft(26, '0');
        var checkDigit = CalculateModulo10Recursive(padded);
        return string.Create(CultureInfo.InvariantCulture, $"{padded}{checkDigit}");
    }

    /// <inheritdoc/>
    public Task ProcessInvoiceComplianceAsync(
        Invoice invoice,
        Tenant tenant,
        Contact contact,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(contact);

        var qrIban = !string.IsNullOrWhiteSpace(tenant.QrIban)
            ? tenant.QrIban
            : (tenant.Iban ?? "CH9300762011623852957");
        var scorRef = GenerateScorReference(invoice.InvoiceNumber);

        var creditorInfo =
            $"{tenant.LegalName};{tenant.Country};{tenant.UidNumber ?? "CHE-123.456.789"}";
        var debtorInfo =
            $"{contact.DisplayName};{contact.Country};{contact.UidNumber ?? contact.VatNumber ?? "N/A"}";

        var qrSvg = GenerateSwissQrBillSvg(
            qrIban,
            scorRef,
            invoice.TotalGross,
            invoice.Currency,
            tenant.LegalName,
            contact.DisplayName);

        if (invoice.QrBillData == null)
        {
            invoice.QrBillData = new InvoiceQrBillData
            {
                InvoiceId = invoice.Id,
                QrIban = qrIban,
                ScorReference = scorRef,
                CreditorInfo = creditorInfo,
                DebtorInfo = debtorInfo,
                QrBillSvg = qrSvg,
                CreatedAt = DateTime.UtcNow,
            };
        }
        else
        {
            invoice.QrBillData.QrIban = qrIban;
            invoice.QrBillData.ScorReference = scorRef;
            invoice.QrBillData.CreditorInfo = creditorInfo;
            invoice.QrBillData.DebtorInfo = debtorInfo;
            invoice.QrBillData.QrBillSvg = qrSvg;
            invoice.QrBillData.UpdatedAt = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Calculates Modulo 10 recursive check digit according to ISO 20022 Swiss QR-Bill specification.
    /// </summary>
    /// <param name="input">The 26 digit string.</param>
    /// <returns>The check digit 0-9.</returns>
    private static int CalculateModulo10Recursive(string input)
    {
        int[] table = [0, 9, 4, 6, 8, 2, 7, 1, 3, 5];
        var carry = 0;

        foreach (var c in input)
        {
            var digit = c - '0';
            carry = table[(carry + digit) % 10];
        }

        return (10 - carry) % 10;
    }

    private static string
        GenerateSwissQrBillSvg(
            string iban,
            string scorRef,
            decimal amount,
            string currency,
            string creditor,
            string debtor) => string.Format(
        "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 600 300\" width=\"100%\" height=\"100%\">\r\n  <rect width=\"600\" height=\"300\" fill=\"#ffffff\" stroke=\"#cccccc\" stroke-width=\"1\"/>\r\n  <text x=\"20\" y=\"30\" font-family=\"Arial, sans-serif\" font-size=\"16\" font-weight=\"bold\" fill=\"#000000\">Payment Part (Swiss QR-Bill / Section paiement)</text>\r\n  <line x1=\"20\" y1=\"40\" x2=\"580\" y2=\"40\" stroke=\"#000000\" stroke-width=\"1\"/>\r\n\r\n  <!-- QR placeholder box with Swiss Cross symbol -->\r\n  <rect x=\"20\" y=\"55\" width=\"140\" height=\"140\" fill=\"#000000\"/>\r\n  <rect x=\"30\" y=\"65\" width=\"120\" height=\"120\" fill=\"#ffffff\"/>\r\n  <rect x=\"75\" y=\"110\" width=\"30\" height=\"30\" fill=\"#e60000\"/>\r\n  <rect x=\"86\" y=\"115\" width=\"8\" height=\"20\" fill=\"#ffffff\"/>\r\n  <rect x=\"80\" y=\"121\" width=\"20\" height=\"8\" fill=\"#ffffff\"/>\r\n\r\n  <!-- Currency and Amount -->\r\n  <text x=\"20\" y=\"225\" font-family=\"Arial, sans-serif\" font-size=\"12\" font-weight=\"bold\" fill=\"#000000\">Currency</text>\r\n  <text x=\"20\" y=\"245\" font-family=\"Arial, sans-serif\" font-size=\"14\" fill=\"#000000\">{0}</text>\r\n  <text x=\"100\" y=\"225\" font-family=\"Arial, sans-serif\" font-size=\"12\" font-weight=\"bold\" fill=\"#000000\">Amount</text>\r\n  <text x=\"100\" y=\"245\" font-family=\"Arial, sans-serif\" font-size=\"14\" fill=\"#000000\">{1}</text>\r\n\r\n  <!-- Account / Payable to -->\r\n  <text x=\"190\" y=\"70\" font-family=\"Arial, sans-serif\" font-size=\"11\" font-weight=\"bold\" fill=\"#000000\">Account / Payable to (QR-IBAN)</text>\r\n  <text x=\"190\" y=\"88\" font-family=\"Arial, sans-serif\" font-size=\"13\" font-weight=\"bold\" fill=\"#000000\">{2}</text>\r\n  <text x=\"190\" y=\"106\" font-family=\"Arial, sans-serif\" font-size=\"12\" fill=\"#000000\">{3}</text>\r\n\r\n  <!-- Reference -->\r\n  <text x=\"190\" y=\"135\" font-family=\"Arial, sans-serif\" font-size=\"11\" font-weight=\"bold\" fill=\"#000000\">Reference (SCOR)</text>\r\n  <text x=\"190\" y=\"153\" font-family=\"Arial, sans-serif\" font-size=\"13\" font-family=\"monospace\" fill=\"#000000\">{4}</text>\r\n\r\n  <!-- Payable by -->\r\n  <text x=\"190\" y=\"182\" font-family=\"Arial, sans-serif\" font-size=\"11\" font-weight=\"bold\" fill=\"#000000\">Payable by (Debtor)</text>\r\n  <text x=\"190\" y=\"200\" font-family=\"Arial, sans-serif\" font-size=\"12\" fill=\"#000000\">{5}</text>\r\n</svg>",
        currency,
        amount.ToString("F2", CultureInfo.InvariantCulture),
        iban,
        creditor,
        scorRef,
        debtor);
}
