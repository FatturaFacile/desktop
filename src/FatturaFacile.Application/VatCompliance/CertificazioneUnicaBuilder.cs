// <copyright file="CertificazioneUnicaBuilder.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Domain.Entities;

namespace FatturaFacile.Application.VatCompliance;

/// <summary>
/// Builds the Certificazioni Uniche of a year from the withholdings made on the payments to professionals and agents.
/// </summary>
public static class CertificazioneUnicaBuilder
{
    /// <summary>
    /// Builds one certificate per percipient and causale.
    /// </summary>
    /// <param name="year">The year of the payments.</param>
    /// <param name="lines">The withheld payments of the year.</param>
    /// <param name="contacts">The suppliers by identifier.</param>
    /// <param name="payments">The Modello F24 payments of the year, to tell which withholdings were paid.</param>
    /// <returns>The certificates in Draft state.</returns>
    public static IReadOnlyList<WithholdingCertificate> Build(
        int year,
        IReadOnlyList<WithholdingLine> lines,
        IReadOnlyDictionary<Guid, Contact> contacts,
        IReadOnlyList<WithholdingF24Payment> payments)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(contacts);
        ArgumentNullException.ThrowIfNull(payments);

        var paid = payments.Where(static p => p.PaidDate is not null).Select(static p => (p.Year, p.Month, p.TaxCode)).ToHashSet();
        List<WithholdingCertificate> certificates = [];

        foreach (var group in lines.Where(l => l.Year == year).GroupBy(static l => (l.SupplierId, l.Reason)))
        {
            var contact = contacts.GetValueOrDefault(group.Key.SupplierId);
            certificates.Add(new WithholdingCertificate
            {
                Year = year,
                SupplierId = group.Key.SupplierId,
                RecipientName = NameOf(contact),
                RecipientTaxCode = contact?.FiscalCode ?? contact?.VatNumber ?? string.Empty,
                RecipientVatNumber = contact?.VatNumber,
                Reason = group.Key.Reason,
                GrossPaid = group.Sum(static l => l.GrossPaid),
                TaxableAmount = group.Sum(static l => l.TaxableAmount),
                WithheldAmount = group.Sum(static l => l.WithheldAmount),
                WithheldPaidAmount = group.Where(l => paid.Contains((l.Year, l.Month, l.F24Code))).Sum(static l => l.WithheldAmount),
            });
        }

        return [.. certificates.OrderBy(static c => c.RecipientName, StringComparer.OrdinalIgnoreCase).ThenBy(static c => c.Reason, StringComparer.Ordinal)];
    }

    private static string NameOf(Contact? contact) => contact is null ? string.Empty : DisplayName(contact);

    private static string DisplayName(Contact contact) =>
        string.IsNullOrWhiteSpace(contact.CompanyName) ? $"{contact.LastName} {contact.FirstName}".Trim() : contact.CompanyName;
}
