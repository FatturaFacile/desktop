// <copyright file="TranslateExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Data;
using System.Windows.Markup;
using FatturaFacile.Wpf.Common.Services;
using Binding = System.Windows.Data.Binding;

namespace FatturaFacile.Wpf.Common.Markup;

/// <summary>
/// XAML markup extension providing localized text strings bound dynamically to the <see cref="LocalizationManager"/>.
/// </summary>
[MarkupExtensionReturnType(returnType: typeof(string))]
public sealed class TranslateExtension : MarkupExtension
{

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslateExtension"/> class.
    /// </summary>
    public TranslateExtension()
    {
        Key = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslateExtension"/> class with the specified localization resource key.
    /// </summary>
    /// <param name="key">The resource key used to look up translated strings.</param>
    public TranslateExtension(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets or sets the resource key identifying the translated string in localization catalogs.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Provides a dynamic data binding to the localized resource or returns an empty string when no key is specified.
    /// </summary>
    /// <param name="serviceProvider">A service provider helper that provides services for the markup extension.</param>
    /// <returns>A binding expression providing the localized string, or <see cref="string.Empty"/> if the key is null or whitespace.</returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(value: Key))
        {
            return string.Empty;
        }

        var binding = new Binding(path: $"Item[{Key}]")
                      {
                          Source = LocalizationManager.Instance,
                          Mode   = BindingMode.OneWay,
                      };

        return binding.ProvideValue(serviceProvider: serviceProvider);
    }

}
