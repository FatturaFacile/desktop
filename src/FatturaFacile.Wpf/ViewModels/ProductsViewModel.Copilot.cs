// <copyright file="ProductsViewModel.Copilot.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Application.Copilot;
using FatturaFacile.Wpf.ViewModels.Copilot;

namespace FatturaFacile.Wpf.ViewModels;

/// <content>
/// Copilot integration of the products module: the selected product is the context and a cited product can be selected.
/// </content>
internal sealed partial class ProductsViewModel : ICopilotEntityTarget
{
    /// <inheritdoc />
    public CopilotContext GetCopilotContext() =>
        new(CopilotRouteResolver.RouteOf("Products"), "product", SelectedProduct?.Id.ToString());

    /// <inheritdoc />
    public Task<bool> SelectEntityAsync(string? entityType, string? entityId)
    {
        var found = Guid.TryParse(entityId, out var id) ? Products.FirstOrDefault(p => p.Id == id) : null;

        if (found is not null)
        {
            SelectedProduct = found;
        }

        return Task.FromResult(found is not null);
    }
}
