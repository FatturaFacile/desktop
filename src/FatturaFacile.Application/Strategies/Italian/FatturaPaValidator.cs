// <copyright file="FatturaPaValidator.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.Strategies.Italian;

/// <summary>
/// Pre-transmission validation of the data needed for a FatturaPA file (mandatory blocks and the most common SDI
/// rejection checks). It complements, but does not replace, validation against the official XSD schema.
/// </summary>
public static class FatturaPaValidator
{
    /// <summary>
    /// Validates the invoice, issuer and customer data.
    /// </summary>
    /// <param name="invoice">The invoice.</param>
    /// <param name="tenant">The issuer (cedente/prestatore).</param>
    /// <param name="contact">The customer (cessionario/committente).</param>
    /// <returns>The list of errors (empty when valid).</returns>
    public static IReadOnlyList<string> Validate(Invoice invoice, Tenant tenant, Contact contact)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(tenant);
        ArgumentNullException.ThrowIfNull(contact);

        List<string> errors = [];

        ValidateIssuer(tenant, errors);
        ValidateCustomer(contact, errors);
        ValidateDocument(invoice, contact, errors);

        return errors;
    }

    private static void ValidateIssuer(Tenant tenant, List<string> errors)
    {
        var vat = FatturaPaRules.CleanVatNumber(tenant.VatNumber);

        if (vat.Length != 11 || !vat.All(char.IsAsciiDigit))
        {
            errors.Add("Partita IVA del cedente mancante o non valida (11 cifre).");
        }

        if (string.IsNullOrWhiteSpace(tenant.LegalName) && string.IsNullOrWhiteSpace(tenant.Name))
        {
            errors.Add("Denominazione del cedente mancante.");
        }

        if (string.IsNullOrWhiteSpace(tenant.TaxRegime) || !tenant.TaxRegime.StartsWith("RF", StringComparison.Ordinal))
        {
            errors.Add("Regime fiscale del cedente mancante (RF01..RF19).");
        }

        ValidateItalianAddress(tenant.Street, tenant.PostalCode, tenant.City, tenant.Province, "cedente", errors);
    }

    private static void ValidateCustomer(Contact contact, List<string> errors)
    {
        var isItalian = string.Equals(contact.Country, "IT", StringComparison.OrdinalIgnoreCase);

        if (isItalian)
        {
            var vat = FatturaPaRules.CleanVatNumber(contact.VatNumber);
            var fiscalCode = (contact.FiscalCode ?? string.Empty).Trim();
            var hasVat = vat.Length == 11 && vat.All(char.IsAsciiDigit);
            var hasFiscalCode = fiscalCode.Length is 11 or 16;

            if (!hasVat && !hasFiscalCode)
            {
                errors.Add("Il cessionario italiano deve avere Partita IVA (11 cifre) o Codice Fiscale (11 o 16 caratteri).");
            }

            ValidateItalianAddress(contact.Street, contact.PostalCode, contact.City, contact.Province, "cessionario", errors);
        }
        else if (string.IsNullOrWhiteSpace(contact.Street) || string.IsNullOrWhiteSpace(contact.City))
        {
            errors.Add("Indirizzo e comune del cessionario estero obbligatori.");
        }

        if (string.IsNullOrWhiteSpace(contact.CompanyName) &&
            (string.IsNullOrWhiteSpace(contact.FirstName) || string.IsNullOrWhiteSpace(contact.LastName)))
        {
            errors.Add("Il cessionario deve avere una denominazione oppure nome e cognome.");
        }

        var recipientCode = FatturaPaRules.ResolveRecipientCode(contact);

        if (contact.IsPublicAdministration && recipientCode.Length != 6)
        {
            errors.Add("Per la Pubblica Amministrazione serve il Codice Univoco Ufficio di 6 caratteri (formato FPA12).");
        }
        else if (!contact.IsPublicAdministration && recipientCode.Length != 7)
        {
            errors.Add("Il Codice Destinatario deve essere di 7 caratteri (o 0000000).");
        }
    }

    private static void ValidateDocument(Invoice invoice, Contact contact, List<string> errors)
    {
        if (FatturaPaRules.GetDocumentType(invoice.InvoiceType) is null)
        {
            errors.Add($"Il tipo documento '{invoice.InvoiceType}' non può essere trasmesso allo SDI.");
        }

        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber) || invoice.InvoiceNumber.Length > 20 ||
            !invoice.InvoiceNumber.Any(char.IsAsciiDigit))
        {
            errors.Add("Il numero fattura è obbligatorio, max 20 caratteri e deve contenere almeno una cifra.");
        }

        if (invoice.Items.Count == 0)
        {
            errors.Add("La fattura deve avere almeno una riga (DettaglioLinee).");
        }

        foreach (var item in invoice.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Description) || item.Description.Length > 1000)
            {
                errors.Add("Descrizione riga mancante o oltre 1000 caratteri.");
            }

            if (item.TaxRate == 0m && (item.VatNature is null || !FatturaPaRules.ValidNatures.Contains(item.VatNature)))
            {
                errors.Add($"La riga '{item.Description}' ha aliquota 0%: indicare una Natura valida (N1..N7).");
            }

            if (item.TaxRate != 0m && item.VatNature is not null)
            {
                errors.Add($"La riga '{item.Description}' ha aliquota diversa da zero: la Natura non è ammessa.");
            }

            if (invoice.ReverseCharge && item.TaxRate != 0m)
            {
                errors.Add($"Fattura in reverse charge: la riga '{item.Description}' deve avere aliquota 0% e Natura N6.x.");
            }
        }

        if (invoice.SplitPayment && invoice.Items.All(static i => i.TaxRate == 0m))
        {
            errors.Add("La scissione dei pagamenti richiede almeno una riga con IVA.");
        }

        if (invoice.WithholdingTaxEnabled && (invoice.WithholdingTaxPercent is null or <= 0m))
        {
            errors.Add("Ritenuta d'acconto attiva: indicare l'aliquota della ritenuta.");
        }

        if (!string.IsNullOrWhiteSpace(contact.Pec) && !contact.Pec.Contains('@', StringComparison.Ordinal))
        {
            errors.Add("Indirizzo PEC del destinatario non valido.");
        }
    }

    private static void ValidateItalianAddress(
        string? street,
        string? postalCode,
        string? city,
        string? province,
        string role,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            errors.Add($"Indirizzo della sede del {role} mancante.");
        }

        if (postalCode is not { Length: 5 } || !postalCode.All(char.IsAsciiDigit))
        {
            errors.Add($"CAP del {role} mancante o non valido (5 cifre).");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            errors.Add($"Comune del {role} mancante.");
        }

        if (province is not { Length: 2 } || !province.All(char.IsAsciiLetter))
        {
            errors.Add($"Provincia del {role} mancante o non valida (sigla di 2 lettere).");
        }
    }
}
