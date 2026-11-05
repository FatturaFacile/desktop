// <copyright file="LocExtension.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows.Data;
using System.Windows.Markup;

namespace FatturaFacile.Wpf.Localization;

/// <summary>
/// Dynamic WPF markup extension enabling real-time UI string localization without restart.
/// </summary>
[ContentProperty(nameof(Key))]
internal sealed class LocExtension : MarkupExtension
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocExtension"/> class.
    /// </summary>
    public LocExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocExtension"/> class with the specified resource key.
    /// </summary>
    /// <param name="key">The localization resource key.</param>
    public LocExtension(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets or sets the localization resource key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new Binding($"[{Key}]")
        {
            Source = LocalizationService.Instance,
            Mode = BindingMode.OneWay,
        };

        return binding.ProvideValue(serviceProvider);
    }
}
