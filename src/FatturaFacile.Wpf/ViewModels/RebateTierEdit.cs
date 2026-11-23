// <copyright file="RebateTierEdit.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace FatturaFacile.Wpf.ViewModels;

/// <summary>
/// An editable tier of the scale in the agreement form: the amounts are typed as text and parsed on save.
/// </summary>
internal sealed partial class RebateTierEdit : ObservableObject
{
    [ObservableProperty]
    private string _threshold = string.Empty;

    [ObservableProperty]
    private string _rate = string.Empty;

    [ObservableProperty]
    private string _flat = string.Empty;
}
