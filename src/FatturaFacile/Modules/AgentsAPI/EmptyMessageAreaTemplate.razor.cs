// <copyright file="EmptyMessageAreaTemplate.razor.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;

namespace FatturaFacile.Modules.AgentsAPI; 
public partial class EmptyMessageAreaTemplate : ComponentBase {
    [Parameter] public string Icon { get; set; }
    [Parameter] public string Title { get; set; }
    [Parameter] public string Description { get; set; }
}
