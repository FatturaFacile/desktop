// <copyright file="SwissQrBillPayloadBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Globalization;
using FatturaFacile.Application.Common.Validation;

namespace FatturaFacile.Application.Strategies.Swiss;

/// <summary>
/// Validates a <see cref="SwissQrBillRequest"/> and builds the Swiss QR code payload (QRType SPC, version 0200, coding 1).
/// </summary>
public static class SwissQrBillPayloadBuilder
{
    /// <summary>
    /// The line separator used between payload elements.
    /// </summary>
    public const string Separator = "\r\n";

    private const decimal MinAmount = 0.01m;
    private const decimal MaxAmount = 999_999_999.99m;

    /// <summary>
    /// Validates the request against the Swiss Implementation Guidelines QR-bill business rules.
    /// </summary>
    /// <param name="request">The QR-bill request.</param>
    /// <returns>The list of validation errors (empty when valid).</returns>
    public static IReadOnlyList<string> Validate(SwissQrBillRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<string> errors = [];

        if (!IbanValidator.IsSwissOrLiechtensteinIban(request.Account))
        {
            errors.Add("Il conto del creditore deve essere un IBAN o QR-IBAN valido svizzero o del Liechtenstein.");
        }

        var isQrIban = IbanValidator.IsSwissQrIban(request.Account);

        switch (request.ReferenceType)
        {
            case "QRR":
                if (!isQrIban)
                {
                    errors.Add("Il riferimento QR (QRR) richiede un QR-IBAN.");
                }

                if (!SwissPaymentReference.IsValidQrReference(request.Reference))
                {
                    errors.Add("Riferimento QR non valido (27 cifre con cifra di controllo modulo 10 ricorsivo).");
                }

                break;

            case "SCOR":
                if (isQrIban)
                {
                    errors.Add("Con un QR-IBAN è obbligatorio il riferimento QR (QRR).");
                }

                if (!SwissPaymentReference.IsValidCreditorReference(request.Reference))
                {
                    errors.Add("Riferimento creditore ISO 11649 (SCOR) non valido.");
                }

                break;

            case "NON":
                if (isQrIban)
                {
                    errors.Add("Con un QR-IBAN è obbligatorio il riferimento QR (QRR).");
                }

                if (!string.IsNullOrEmpty(request.Reference))
                {
                    errors.Add("Il tipo di riferimento NON non ammette un riferimento.");
                }

                break;

            default:
                errors.Add("Tipo di riferimento sconosciuto (ammessi: QRR, SCOR, NON).");
                break;
        }

        if (!string.Equals(request.Currency, "CHF", StringComparison.Ordinal) &&
            !string.Equals(request.Currency, "EUR", StringComparison.Ordinal))
        {
            errors.Add("La valuta della QR-bill deve essere CHF o EUR.");
        }

        if (request.Amount is { } amount && (amount < MinAmount || amount > MaxAmount))
        {
            errors.Add("L'importo deve essere compreso tra 0.01 e 999'999'999.99.");
        }

        if ((request.UnstructuredMessage?.Length ?? 0) > 140)
        {
            errors.Add("Le informazioni supplementari non possono superare 140 caratteri.");
        }

        ValidateAddress(request.Creditor, "creditore", errors);

        if (request.Debtor is not null)
        {
            ValidateAddress(request.Debtor, "debitore", errors);
        }

        return errors;
    }

    /// <summary>
    /// Builds the Swiss QR code payload. The request must be valid (see <see cref="Validate"/>).
    /// </summary>
    /// <param name="request">The QR-bill request.</param>
    /// <returns>The payload string (elements separated by CR+LF, no trailing separator).</returns>
    public static string Build(SwissQrBillRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        List<string> lines =
        [
            "SPC",
            "0200",
            "1",
            IbanValidator.Normalize(request.Account),
        ];

        AppendAddress(lines, request.Creditor);

        // Ultimate creditor: reserved for future use, must be delivered empty (7 elements).
        AppendAddress(lines, address: null);

        lines.Add(request.Amount?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty);
        lines.Add(request.Currency);

        AppendAddress(lines, request.Debtor);

        lines.Add(request.ReferenceType);
        lines.Add(Clean(request.Reference).Replace(" ", string.Empty, StringComparison.Ordinal));
        lines.Add(Clean(request.UnstructuredMessage));
        lines.Add("EPD");

        return string.Join(Separator, lines);
    }

    private static void AppendAddress(List<string> lines, SwissQrBillAddress? address)
    {
        if (address is null)
        {
            lines.AddRange(Enumerable.Repeat(string.Empty, 7));
            return;
        }

        lines.Add("S");
        lines.Add(Clean(address.Name));
        lines.Add(Clean(address.Street));
        lines.Add(Clean(address.HouseNumber));
        lines.Add(Clean(address.PostalCode));
        lines.Add(Clean(address.Town));
        lines.Add(Clean(address.Country).ToUpperInvariant());
    }

    private static void ValidateAddress(SwissQrBillAddress address, string role, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(address.Name) || address.Name.Length > 70)
        {
            errors.Add($"Nome del {role} mancante o oltre 70 caratteri.");
        }

        if ((address.Street?.Length ?? 0) > 70 || (address.HouseNumber?.Length ?? 0) > 16)
        {
            errors.Add($"Via (max 70) o numero civico (max 16) del {role} troppo lunghi.");
        }

        if (string.IsNullOrWhiteSpace(address.PostalCode) || address.PostalCode.Length > 16)
        {
            errors.Add($"NPA/CAP del {role} mancante o oltre 16 caratteri.");
        }

        if (string.IsNullOrWhiteSpace(address.Town) || address.Town.Length > 35)
        {
            errors.Add($"Località del {role} mancante o oltre 35 caratteri.");
        }

        if (address.Country is not { Length: 2 } || !address.Country.All(char.IsAsciiLetter))
        {
            errors.Add($"Codice paese ISO a 2 lettere del {role} non valido.");
        }
    }

    private static string Clean(string? value) => (value ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ').Trim();
}
