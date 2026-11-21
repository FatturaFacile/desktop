// <copyright file="InvoicingViewModel.Copilot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;
using FatturaFacile.Wpf.ViewModels.Copilot;

namespace FatturaFacile.Wpf.ViewModels;

/// <content>
/// Copilot integration of the invoicing module: the selected invoice is the context and a cited invoice can be selected.
/// </content>
internal sealed partial class InvoicingViewModel : ICopilotEntityTarget
{
    /// <inheritdoc />
    public CopilotContext GetCopilotContext() =>
        new(CopilotRouteResolver.RouteOf("Invoicing"), "invoice", SelectedInvoice?.Id.ToString());

    /// <inheritdoc />
    public Task<bool> SelectEntityAsync(string? entityType, string? entityId)
    {
        var found = Guid.TryParse(entityId, out var id) ? Invoices.FirstOrDefault(i => i.Id == id) : null;

        if (found is not null)
        {
            SelectedInvoice = found;
        }

        return Task.FromResult(found is not null);
    }
}
