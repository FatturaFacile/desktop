// <copyright file="TemplateAccountInput.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;
using FatturaFacile.Application.DTOs;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// The account the user picks for one account parameter of the selected template.
/// </summary>
/// <param name="key">The parameter key of the template.</param>
internal sealed partial class TemplateAccountInput(string key) : ObservableObject
{
    [ObservableProperty]
    private ChartOfAccountDto? _account;

    /// <summary>
    /// Gets the parameter key of the template.
    /// </summary>
    public string Key { get; } = key;
}
