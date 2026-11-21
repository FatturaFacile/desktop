// <copyright file="ContactsViewModel.Copilot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;
using FatturaFacile.Wpf.ViewModels.Copilot;

namespace FatturaFacile.Wpf.ViewModels;

/// <content>
/// Copilot integration of the contacts module: the selected contact is the context and a cited contact can be selected.
/// </content>
internal sealed partial class ContactsViewModel : ICopilotEntityTarget
{
    /// <inheritdoc />
    public CopilotContext GetCopilotContext() =>
        new(CopilotRouteResolver.RouteOf("Contacts"), "contact", SelectedContact?.Id.ToString());

    /// <inheritdoc />
    public Task<bool> SelectEntityAsync(string? entityType, string? entityId)
    {
        var found = Guid.TryParse(entityId, out var id) ? Contacts.FirstOrDefault(c => c.Id == id) : null;

        if (found is not null)
        {
            SelectedContact = found;
        }

        return Task.FromResult(found is not null);
    }
}
