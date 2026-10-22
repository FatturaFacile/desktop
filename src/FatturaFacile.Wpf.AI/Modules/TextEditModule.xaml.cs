// <copyright file="TextEditModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Media.Imaging;

using DevExpress.Xpf.DemoBase.Helpers;
using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module illustrating text manipulation, summarization, proofreading, and rewriting actions on a TextEdit control.
/// </summary>
public partial class TextEditModule
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TextEditModule"/> class and sets up initial sample content.
    /// </summary>
    public TextEditModule()
    {
        InitializeComponent();

        WarningText            = Resource.EndpointWarning;
        mainTextEdit.EditValue = Resource.TextEditContent;

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        var assembly = typeof(TextEditModule).Assembly;

        bitmapImage.StreamSource = assembly.GetManifestResourceStream(
                                                                      name: DemoHelper.GetPath(
                                                                                               path: $"{assembly.GetName().Name}.Resources.",
                                                                                               assembly: assembly) +
                                                                            "ai_assistant_menu.png");

        bitmapImage.EndInit();
        image.Source = bitmapImage;
    }

}
