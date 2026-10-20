using System.Globalization;
using System.Text;
using FatturaFacile.Domain.Entities.Sales;

namespace FatturaFacile.Infrastructure.CrossBorder.SwissQr;

public static class SwissQrBillGenerator
{
    /// <summary>
    /// Generates standard Swiss QR Code Data Payload according to Swiss Implementation Guidelines.
    /// </summary>
    /// <returns></returns>
    public static string GenerateQrPayload(SwissQrBill bill)
    {
        var sb = new StringBuilder();

        // 1. Header
        sb.Append("SPC\n");       // Swiss Payments Code
        sb.Append("0200\n");      // Version 2.0
        sb.Append("1\n");         // Coding Type: 1 = UTF-8

        // 2. Creditor Info
        sb.Append(bill.CreditorQrIban.Value).Append('\n');
        sb.Append("S\n");         // Address Type: S = Structured
        sb.Append(bill.CreditorName).Append('\n');
        sb.Append(bill.CreditorAddress.Street).Append('\n');
        sb.Append(bill.CreditorAddress.PostalCode).Append('\n');
        sb.Append(bill.CreditorAddress.City).Append('\n');
        sb.Append(bill.CreditorAddress.CountryCode).Append('\n');

        // 3. Ultimate Creditor (Empty reserved lines)
        sb.Append("\n\n\n\n\n\n\n");

        // 4. Payment Amount & Currency
        sb.Append(bill.Amount.Amount.ToString("0.00", CultureInfo.InvariantCulture)).Append('\n');
        sb.Append(bill.Amount.Currency.ToString()).Append('\n');

        // 5. Ultimate Debtor
        sb.Append("S\n");
        sb.Append(bill.DebtorName).Append('\n');
        sb.Append(bill.DebtorAddress.Street).Append('\n');
        sb.Append(bill.DebtorAddress.PostalCode).Append('\n');
        sb.Append(bill.DebtorAddress.City).Append('\n');
        sb.Append(bill.DebtorAddress.CountryCode).Append('\n');

        // 6. Reference Type & SCOR Reference
        sb.Append("QRR\n");       // QRR = QR Reference (27 digits)
        sb.Append(FormatScorReference(bill.ReferenceNumber)).Append('\n');

        // 7. Unstructured Message & Trailer
        sb.Append(bill.AdditionalUnstructuredMessage ?? string.Empty).Append('\n');
        sb.Append("EPD\n");       // End Payment Data

        return sb.ToString();
    }

    /// <summary>
    /// Formats 27-digit SCOR reference into 6 blocks of 5 digits and 1 block of 2 digits (e.g., 21 00000 00003 13947 11960 09917).
    /// </summary>
    /// <returns></returns>
    public static string FormatScorReference(string rawRef)
    {
        var digitsOnly = new string([.. rawRef.Where(char.IsDigit)]);
        if (digitsOnly.Length < 27)
        {
            digitsOnly = digitsOnly.PadLeft(27, '0');
        }

        var sb = new StringBuilder();
        for (var i = 0; i < digitsOnly.Length; i++)
        {
            if (i > 0 && i % 5 == 2)
            {
                sb.Append(' ');
            }

            sb.Append(digitsOnly[i]);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Computes the Modulo 10 recursive checksum digit for a Swiss QR reference.
    /// </summary>
    /// <returns></returns>
    public static int CalculateModulo10(string input)
    {
        var table = new int[] { 0, 9, 4, 6, 8, 2, 7, 1, 3, 5 };
        var carry = 0;

        foreach (var c in input)
        {
            if (char.IsDigit(c))
            {
                var digit = c - '0';
                carry = table[(carry + digit) % 10];
            }
        }

        return (10 - carry) % 10;
    }

    /// <summary>
    /// Generates SVG markup for the Swiss QR-bill preview with the iconic Swiss Cross badge.
    /// </summary>
    /// <returns></returns>
    public static string GenerateSvgPreview(SwissQrBill bill)
    {
        var scor = FormatScorReference(bill.ReferenceNumber);
        var amountStr = bill.Amount.Amount.ToString("N2", new CultureInfo("de-CH"));
        var currencyStr = bill.Amount.Currency.ToString();
        var additionalMsg = bill.AdditionalUnstructuredMessage ?? "FatturaFacile ERP Svizzera";

        return $$"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 400" width="100%" height="100%">
  <defs>
    <style>
      .title { font-family: 'Segoe UI', Arial, sans-serif; font-size: 16px; font-weight: bold; fill: #111827; }
      .label { font-family: 'Segoe UI', Arial, sans-serif; font-size: 11px; font-weight: bold; fill: #374151; }
      .value { font-family: 'Segoe UI', Arial, sans-serif; font-size: 11px; fill: #1f2937; }
      .amount { font-family: 'Segoe UI', Arial, sans-serif; font-size: 14px; font-weight: bold; fill: #111827; }
      .scor { font-family: 'Courier New', monospace; font-size: 12px; font-weight: bold; fill: #111827; }
    </style>
  </defs>

  <!-- Border & Background -->
  <rect x="0" y="0" width="800" height="400" fill="#ffffff" stroke="#d1d5db" stroke-width="1" rx="8" />
  
  <!-- Receipt Part (Left) -->
  <line x1="240" y1="0" x2="240" y2="400" stroke="#9ca3af" stroke-width="1" stroke-dasharray="5,5" />
  <text x="20" y="30" class="title">Ricevuta</text>
  
  <text x="20" y="60" class="label">Conto / Pagabile a</text>
  <text x="20" y="75" class="value">{{bill.CreditorQrIban.ToFormattedString()}}</text>
  <text x="20" y="90" class="value">{{bill.CreditorName}}</text>
  <text x="20" y="105" class="value">{{bill.CreditorAddress.Street}}</text>
  <text x="20" y="120" class="value">{{bill.CreditorAddress.PostalCode}} {{bill.CreditorAddress.City}}</text>

  <text x="20" y="150" class="label">Riferimento</text>
  <text x="20" y="165" class="scor">{{scor}}</text>

  <text x="20" y="195" class="label">Pagabile da</text>
  <text x="20" y="210" class="value">{{bill.DebtorName}}</text>
  <text x="20" y="225" class="value">{{bill.DebtorAddress.Street}}</text>
  <text x="20" y="240" class="value">{{bill.DebtorAddress.PostalCode}} {{bill.DebtorAddress.City}}</text>

  <text x="20" y="340" class="label">Valuta</text>
  <text x="70" y="340" class="label">Importo</text>
  <text x="20" y="365" class="amount">{{currencyStr}}</text>
  <text x="70" y="365" class="amount">{{amountStr}}</text>

  <!-- Payment Part (Right) -->
  <text x="260" y="30" class="title">Sezione pagamento</text>

  <!-- Swiss QR Code Placeholder with Swiss Cross Emblem -->
  <rect x="260" y="50" width="160" height="160" fill="#000000" rx="4" />
  <rect x="270" y="60" width="140" height="140" fill="#ffffff" />
  <!-- QR pattern simulation -->
  <rect x="275" y="65" width="35" height="35" fill="#000000" />
  <rect x="280" y="70" width="25" height="25" fill="#ffffff" />
  <rect x="287" y="77" width="11" height="11" fill="#000000" />
  <rect x="370" y="65" width="35" height="35" fill="#000000" />
  <rect x="375" y="70" width="25" height="25" fill="#ffffff" />
  <rect x="382" y="77" width="11" height="11" fill="#000000" />
  <rect x="275" y="160" width="35" height="35" fill="#000000" />
  <rect x="280" y="165" width="25" height="25" fill="#ffffff" />
  <rect x="287" y="172" width="11" height="11" fill="#000000" />
  <!-- Center Swiss Cross badge -->
  <rect x="326" y="116" width="28" height="28" fill="#000000" />
  <rect x="328" y="118" width="24" height="24" fill="#e11d48" />
  <rect x="338" y="122" width="4" height="16" fill="#ffffff" />
  <rect x="332" y="128" width="16" height="4" fill="#ffffff" />

  <!-- Right Details -->
  <text x="450" y="60" class="label">Conto / Pagabile a</text>
  <text x="450" y="75" class="value">{{bill.CreditorQrIban.ToFormattedString()}}</text>
  <text x="450" y="90" class="value">{{bill.CreditorName}}</text>
  <text x="450" y="105" class="value">{{bill.CreditorAddress.Street}}</text>
  <text x="450" y="120" class="value">{{bill.CreditorAddress.PostalCode}} {{bill.CreditorAddress.City}}</text>

  <text x="450" y="150" class="label">Riferimento</text>
  <text x="450" y="168" class="scor">{{scor}}</text>

  <text x="450" y="195" class="label">Informazioni supplementari</text>
  <text x="450" y="210" class="value">{{additionalMsg}}</text>

  <text x="450" y="240" class="label">Pagabile da</text>
  <text x="450" y="255" class="value">{{bill.DebtorName}}</text>
  <text x="450" y="270" class="value">{{bill.DebtorAddress.Street}}</text>
  <text x="450" y="285" class="value">{{bill.DebtorAddress.PostalCode}} {{bill.DebtorAddress.City}}</text>

  <!-- Amount Bar -->
  <text x="260" y="340" class="label">Valuta</text>
  <text x="320" y="340" class="label">Importo</text>
  <text x="260" y="365" class="amount">{{currencyStr}}</text>
  <text x="320" y="365" class="amount">{{amountStr}}</text>
</svg>
""";
    }
}
