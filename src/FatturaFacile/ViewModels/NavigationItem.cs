// <copyright file="NavigationItem.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System;

using Common;

using DevExpress.Mvvm;

namespace FatturaFacile.ViewModels;

[Serializable]
public class NavigationItem : INavigationItem, ISupportState<NavigationItem>
{
    public string Caption { get; set; } = string.Empty;
    public NavigationItem() { }
    public NavigationItem(string caption)
    {
        Caption = caption;
    }

    #region Serialization
    NavigationItem ISupportState<NavigationItem>.SaveState()
    {
        return this;
    }
    void ISupportState<NavigationItem>.RestoreState(NavigationItem state)
    {
        Caption = state.Caption;
    }
    #endregion
}
