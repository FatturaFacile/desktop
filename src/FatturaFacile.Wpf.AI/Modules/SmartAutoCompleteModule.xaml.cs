// <copyright file="SmartAutoCompleteModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic for the Smart AutoComplete demonstration module.
/// </summary>
public partial class SmartAutoCompleteModule
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartAutoCompleteModule"/> class.
    /// </summary>
    public SmartAutoCompleteModule()
    {
        InitializeComponent();

        WarningText = Resource.EndpointWarning;
    }

}

/// <summary>
/// Represents a user role selectable item in smart autocomplete dropdowns.
/// </summary>
public class UserRoleItem
{

    /// <summary>Gets or sets the user role key name.</summary>
    public string UserRole { get; set; } = string.Empty;

    /// <summary>Gets or sets the user role display text.</summary>
    public string DisplayText { get; set; } = string.Empty;

}
