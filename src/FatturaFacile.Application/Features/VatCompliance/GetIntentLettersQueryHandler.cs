// <copyright file="GetIntentLettersQueryHandler.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Common.Interfaces;
using FatturaFacile.Application.VatCompliance;
using FatturaFacile.Domain.Entities;
using MediatR;

namespace FatturaFacile.Application.Features.VatCompliance;

/// <summary>
/// Handles <see cref="GetIntentLettersQuery"/>.
/// </summary>
/// <param name="unitOfWork">The unit of work.</param>
public sealed class GetIntentLettersQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetIntentLettersQuery, IReadOnlyList<IntentLetterView>>
{
    /// <inheritdoc/>
    public Task<IReadOnlyList<IntentLetterView>> Handle(GetIntentLettersQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return HandleInternalAsync(request, cancellationToken);
    }

    private static IntentLetterView ViewOf(IntentLetter letter, Contact? contact, decimal used) =>
        new(letter, NameOf(contact), IntentLetterPlafond.Consumed(letter, used), IntentLetterPlafond.Residual(letter, used));

    private static string NameOf(Contact? contact) => contact is null ? string.Empty : DisplayName(contact);

    private static string DisplayName(Contact contact) =>
        string.IsNullOrWhiteSpace(contact.CompanyName) ? $"{contact.FirstName} {contact.LastName}".Trim() : contact.CompanyName;

    private async Task<IReadOnlyList<IntentLetterView>> HandleInternalAsync(GetIntentLettersQuery request, CancellationToken cancellationToken)
    {
        var (letters, usage) = await IntentLetterLookup.LoadAsync(unitOfWork, cancellationToken).ConfigureAwait(false);
        var shown = letters.Where(l => request.Year is null || l.Year == request.Year).OrderByDescending(static l => l.ReceiptDate).ToList();
        var contacts = await IntentLetterLookup.ContactsAsync(unitOfWork, shown.Select(static l => l.ContactId), cancellationToken).ConfigureAwait(false);

        return [.. shown.Select(l => ViewOf(l, contacts.GetValueOrDefault(l.ContactId), usage.GetValueOrDefault(l.Id)))];
    }
}
