// <copyright file="TestDocumentModule.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using DevExpress.Mvvm;
using DevExpress.Mvvm.ModuleInjection;

namespace FatturaFacile.Tests;

/// <summary>
/// Test implementation of <see cref="IDocumentModule"/> used to verify
/// document activation and navigation behaviour within the module manager.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TestDocumentModule"/> class.
/// </remarks>
/// <param name="key">
/// The unique module key used for navigation and identification.
/// </param>
/// <param name="caption">
/// The display caption associated with the module.
/// </param>
internal class TestDocumentModule(string key, string caption) : BindableBase, IDocumentModule
{

    /// <summary>
    /// The unique module key used for navigation and identification.
    /// </summary>
    private readonly string key = key;

    /// <summary>
    /// Gets the display caption of the module.
    /// </summary>
    public string Caption { get; } = caption;

    /// <summary>
    /// Gets or sets a value indicating whether the document is currently active.
    /// When set to <see langword="true"/>, navigation to the corresponding
    /// document is triggered through the default module manager.
    /// </summary>
    public bool IsActive
    {
        get;
        set
        {
            if (SetProperty(storage: ref field, value: value, propertyName: nameof(IsActive)) && value)
            {
                ModuleManager.DefaultManager?.Navigate(regionName: Common.Regions.Documents, key: key);
            }
        }
    }

}
