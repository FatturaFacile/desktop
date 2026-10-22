// <copyright file="EmptyMessageAreaTemplate.razor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;

namespace FatturaFacile.Modules.AgentsAPI;

/// <summary>
/// Blazor component rendering the placeholder empty state in the chat message area before conversation turns begin.
/// </summary>
public partial class EmptyMessageAreaTemplate : ComponentBase
{

    /// <summary>
    /// Gets or sets the icon CSS class or glyph identifier displayed in the empty state illustration.
    /// </summary>
    [Parameter]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary title header for the empty state greeting.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the informative subtitle or onboarding instruction text.
    /// </summary>
    [Parameter]
    public string Description { get; set; } = string.Empty;

}
