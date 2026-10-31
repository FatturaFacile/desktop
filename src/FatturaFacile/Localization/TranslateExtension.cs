// <copyright file="TranslateExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Data;
using System.Windows.Markup;

namespace FatturaFacile.Wpf.Localization;

/// <summary>
/// Dynamic XAML markup extension allowing instant runtime localization updates across all views.
/// Usage: Text="{loc:Translate Nav_Invoices}"
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public class TranslateExtension : MarkupExtension
{
    /// <summary>
    /// Gets or sets the localization resource key name to bind.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslateExtension"/> class.
    /// </summary>
    public TranslateExtension() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslateExtension"/> class with the specified resource key.
    /// </summary>
    /// <param name="key">The localization resource key.</param>
    public TranslateExtension(string key)
    {
        Key = key;
    }

    /// <inheritdoc />
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
        {
            return string.Empty;
        }

        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationManager.Instance,
            Mode = BindingMode.OneWay
        };

        return binding.ProvideValue(serviceProvider);
    }
}
