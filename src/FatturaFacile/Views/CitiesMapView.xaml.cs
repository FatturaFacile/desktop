// <copyright file="CitiesMapView.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.DevAV.ViewModels;
using DevExpress.Xpf.Charts;
using DevExpress.Xpf.Map;
using System.Windows;
using System.Windows.Controls;

namespace FatturaFacile.Views; 
public partial class CitiesMapView : UserControl {
    public CitiesMapView() {
        InitializeComponent();
    }

    void OnWebRequest(object sender, DevExpress.Xpf.Map.MapWebRequestEventArgs e) {
        e.UserAgent = "DevExpress WPF Map Control Main Demo";
    }
}
