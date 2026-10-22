// <copyright file="PromptToExpressionModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module for translating natural language queries into DevExpress criteria and filter expressions.
/// </summary>
public partial class PromptToExpressionModule
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptToExpressionModule"/> class.
    /// </summary>
    public PromptToExpressionModule()
    {
        InitializeComponent();
        WarningText = Resource.EndpointWarning;
    }

}
