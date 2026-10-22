// <copyright file="TextEditModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Media.Imaging;
using DevExpress.Xpf.DemoBase.Helpers;

namespace FatturaFacile; 
public partial class TextEditModule {
    public TextEditModule() {
        InitializeComponent();

        WarningText = Properties.Resource.EndpointWarning;
        mainTextEdit.EditValue = Properties.Resource.TextEditContent;

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        var assembly = typeof(TextEditModule).Assembly;
        bitmapImage.StreamSource = assembly.GetManifestResourceStream(
            DemoHelper.GetPath($"{assembly.GetName().Name}.Resources.", assembly) + "ai_assistant_menu.png");
        bitmapImage.EndInit();
        image.Source = bitmapImage;
    }
}
