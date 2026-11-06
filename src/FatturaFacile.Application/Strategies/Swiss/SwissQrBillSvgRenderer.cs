// <copyright file="SwissQrBillSvgRenderer.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using System.Net;
using System.Text;
using FatturaFacile.Application.Common.Models;
using FatturaFacile.Application.Common.Validation;

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Renders the Swiss QR-bill (receipt 62 mm + payment part 148 mm, 210 x 105 mm) as SVG in millimetre units,
/// with the 46 x 46 mm Swiss QR code and the 7 x 7 mm Swiss cross (labels in Italian).
/// </summary>
public static class SwissQrBillSvgRenderer
{
    private const decimal QrSize = 46m;
    private const decimal QrX = 67m;
    private const decimal QrY = 17m;
    private const decimal CrossSize = 7m;

    /// <summary>
    /// Renders the QR-bill as an SVG document.
    /// </summary>
    /// <param name="request">The validated QR-bill request.</param>
    /// <param name="modules">The QR code module matrix without quiet zone.</param>
    /// <returns>The SVG markup.</returns>
    public static string Render(SwissQrBillRequest request, QrCodeMatrix modules)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(modules);

        StringBuilder svg = new();
        _ = svg.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"210mm\" height=\"105mm\" viewBox=\"0 0 210 105\" font-family=\"Arial, Helvetica, 'Liberation Sans', sans-serif\">");
        _ = svg.Append("<rect width=\"210\" height=\"105\" fill=\"#ffffff\"/>");
        _ = svg.Append("<line x1=\"0\" y1=\"0.1\" x2=\"210\" y2=\"0.1\" stroke=\"#000000\" stroke-width=\"0.2\" stroke-dasharray=\"1,1\"/>");
        _ = svg.Append("<line x1=\"62\" y1=\"0\" x2=\"62\" y2=\"105\" stroke=\"#000000\" stroke-width=\"0.2\" stroke-dasharray=\"1,1\"/>");

        RenderReceipt(svg, request);
        RenderPaymentPart(svg, request);
        RenderQrCode(svg, modules);

        _ = svg.Append("</svg>");
        return svg.ToString();
    }

    /// <summary>
    /// Formats an IBAN in blocks of four characters.
    /// </summary>
    /// <param name="iban">The IBAN.</param>
    /// <returns>The formatted IBAN.</returns>
    public static string FormatIban(string iban) => FormatBlocks(IbanValidator.Normalize(iban), 4, fromRight: false);

    /// <summary>
    /// Formats a payment reference: QRR in blocks of five from the right, SCOR in blocks of four from the left.
    /// </summary>
    /// <param name="referenceType">The reference type.</param>
    /// <param name="reference">The reference.</param>
    /// <returns>The formatted reference.</returns>
    public static string FormatReference(string referenceType, string reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        var compact = reference.Replace(" ", string.Empty, StringComparison.Ordinal);
        return string.Equals(referenceType, "QRR", StringComparison.Ordinal)
            ? FormatBlocks(compact, 5, fromRight: true)
            : FormatBlocks(compact, 4, fromRight: false);
    }

    /// <summary>
    /// Formats an amount with a blank as thousands separator and two decimals (e.g. "1 949.75").
    /// </summary>
    /// <param name="amount">The amount.</param>
    /// <returns>The formatted amount.</returns>
    public static string FormatAmount(decimal amount)
    {
        var format = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        format.NumberGroupSeparator = " ";
        return amount.ToString("#,##0.00", format);
    }

    private static void RenderReceipt(StringBuilder svg, SwissQrBillRequest request)
    {
        Text(svg, 5m, 10m, 11m, bold: true, "Ricevuta");

        var y = 17m;
        y = Section(svg, 5m, y, "Conto / Pagabile a", AccountLines(request), receipt: true);

        if (!string.Equals(request.ReferenceType, "NON", StringComparison.Ordinal))
        {
            y = Section(svg, 5m, y, "Riferimento", [FormatReference(request.ReferenceType, request.Reference)], receipt: true);
        }

        if (request.Debtor is not null)
        {
            _ = Section(svg, 5m, y, "Pagabile da", AddressLines(request.Debtor), receipt: true);
        }
        else
        {
            Text(svg, 5m, y, 6m, bold: true, "Pagabile da (nome/indirizzo)");
            CornerBox(svg, 5m, y + 1.5m, 52m, 20m);
        }

        Text(svg, 5m, 71m, 6m, bold: true, "Valuta");
        Text(svg, 5m, 75m, 8m, bold: false, request.Currency);
        Text(svg, 20m, 71m, 6m, bold: true, "Importo");

        if (request.Amount is { } amount)
        {
            Text(svg, 20m, 75m, 8m, bold: false, FormatAmount(amount));
        }
        else
        {
            CornerBox(svg, 27m, 69m, 30m, 10m);
        }

        _ = svg.Append("<text x=\"57\" y=\"86\" font-size=\"6pt\" font-weight=\"bold\" text-anchor=\"end\">Punto di accettazione</text>");
    }

    private static void RenderPaymentPart(StringBuilder svg, SwissQrBillRequest request)
    {
        Text(svg, 67m, 10m, 11m, bold: true, "Sezione pagamento");

        Text(svg, 67m, 71m, 8m, bold: true, "Valuta");
        Text(svg, 67m, 76m, 10m, bold: false, request.Currency);
        Text(svg, 85m, 71m, 8m, bold: true, "Importo");

        if (request.Amount is { } amount)
        {
            Text(svg, 85m, 76m, 10m, bold: false, FormatAmount(amount));
        }
        else
        {
            CornerBox(svg, 78m, 73m, 40m, 15m);
        }

        var y = 10m;
        y = Section(svg, 118m, y, "Conto / Pagabile a", AccountLines(request), receipt: false);

        if (!string.Equals(request.ReferenceType, "NON", StringComparison.Ordinal))
        {
            y = Section(svg, 118m, y, "Riferimento", [FormatReference(request.ReferenceType, request.Reference)], receipt: false);
        }

        if (!string.IsNullOrWhiteSpace(request.UnstructuredMessage))
        {
            y = Section(svg, 118m, y, "Informazioni supplementari", [request.UnstructuredMessage], receipt: false);
        }

        if (request.Debtor is not null)
        {
            _ = Section(svg, 118m, y, "Pagabile da", AddressLines(request.Debtor), receipt: false);
        }
        else
        {
            Text(svg, 118m, y, 8m, bold: true, "Pagabile da (nome/indirizzo)");
            CornerBox(svg, 118m, y + 2m, 65m, 25m);
        }
    }

    private static void RenderQrCode(StringBuilder svg, QrCodeMatrix modules)
    {
        var count = modules.Size;

        if (count == 0)
        {
            return;
        }

        var moduleSize = QrSize / count;
        _ = svg.Append("<path fill=\"#000000\" d=\"");

        for (var row = 0; row < count; row++)
        {
            for (var col = 0; col < count; col++)
            {
                if (!modules.IsDark(row, col))
                {
                    continue;
                }

                var x = QrX + (col * moduleSize);
                var y = QrY + (row * moduleSize);
                _ = svg.Append(Invariant($"M{N(x)} {N(y)}h{N(moduleSize)}v{N(moduleSize)}h-{N(moduleSize)}z"));
            }
        }

        _ = svg.Append("\"/>");

        // Swiss cross: 7 x 7 mm, white border, black square, white cross (centred on the QR code).
        const decimal cx = QrX + ((QrSize - CrossSize) / 2m);
        const decimal cy = QrY + ((QrSize - CrossSize) / 2m);
        _ = svg.Append(Invariant($"<rect x=\"{N(cx)}\" y=\"{N(cy)}\" width=\"7\" height=\"7\" fill=\"#ffffff\"/>"));
        _ = svg.Append(Invariant($"<rect x=\"{N(cx + 0.5m)}\" y=\"{N(cy + 0.5m)}\" width=\"6\" height=\"6\" fill=\"#000000\"/>"));
        _ = svg.Append(Invariant($"<rect x=\"{N(cx + 2.92m)}\" y=\"{N(cy + 1.55m)}\" width=\"1.16\" height=\"3.9\" fill=\"#ffffff\"/>"));
        _ = svg.Append(Invariant($"<rect x=\"{N(cx + 1.55m)}\" y=\"{N(cy + 2.92m)}\" width=\"3.9\" height=\"1.16\" fill=\"#ffffff\"/>"));
    }

    private static List<string> AccountLines(SwissQrBillRequest request)
    {
        List<string> lines = [FormatIban(request.Account)];
        lines.AddRange(AddressLines(request.Creditor));
        return lines;
    }

    private static List<string> AddressLines(SwissQrBillAddress address)
    {
        List<string> lines = [address.Name];
        var street = string.IsNullOrWhiteSpace(address.HouseNumber)
            ? (address.Street ?? string.Empty).Trim()
            : $"{address.Street} {address.HouseNumber}".Trim();

        if (!string.IsNullOrWhiteSpace(street))
        {
            lines.Add(street);
        }

        var town = string.Equals(address.Country, "CH", StringComparison.OrdinalIgnoreCase)
            ? $"{address.PostalCode} {address.Town}"
            : $"{address.Country}-{address.PostalCode} {address.Town}";
        lines.Add(town);
        return lines;
    }

    private static decimal Section(
        StringBuilder svg,
        decimal x,
        decimal y,
        string heading,
        IEnumerable<string> values,
        bool receipt)
    {
        var headingSize = receipt ? 6m : 8m;
        var valueSize = receipt ? 8m : 10m;
        var lineHeight = receipt ? 3.5m : 4.2m;
        Text(svg, x, y, headingSize, bold: true, heading);
        var current = y + lineHeight;

        foreach (var value in values)
        {
            Text(svg, x, current, valueSize, bold: false, value);
            current += lineHeight;
        }

        return current + (lineHeight / 2m);
    }

    private static void Text(StringBuilder svg, decimal x, decimal y, decimal sizePt, bool bold, string value)
    {
        var weight = bold ? " font-weight=\"bold\"" : string.Empty;
        _ = svg.Append(Invariant($"<text x=\"{N(x)}\" y=\"{N(y)}\" font-size=\"{N(sizePt)}pt\"{weight}>{WebUtility.HtmlEncode(value)}</text>"));
    }

    private static void CornerBox(StringBuilder svg, decimal x, decimal y, decimal width, decimal height)
    {
        const decimal c = 3m;
        var x2 = x + width;
        var y2 = y + height;
        _ = svg.Append(Invariant(
            $"<path fill=\"none\" stroke=\"#000000\" stroke-width=\"0.26\" d=\"M{N(x)} {N(y + c)}V{N(y)}H{N(x + c)}M{N(x2 - c)} {N(y)}H{N(x2)}V{N(y + c)}M{N(x2)} {N(y2 - c)}V{N(y2)}H{N(x2 - c)}M{N(x + c)} {N(y2)}H{N(x)}V{N(y2 - c)}\"/>"));
    }

    private static string FormatBlocks(string value, int size, bool fromRight)
    {
        StringBuilder result = new();
        var offset = fromRight ? value.Length % size : 0;

        for (var i = 0; i < value.Length; i++)
        {
            if (i > 0 && (i - offset) % size == 0)
            {
                _ = result.Append(' ');
            }

            _ = result.Append(value[i]);
        }

        return result.ToString();
    }

    private static string N(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static string Invariant(FormattableString value) => FormattableString.Invariant(value);
}
