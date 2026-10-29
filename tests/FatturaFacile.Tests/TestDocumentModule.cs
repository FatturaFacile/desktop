// <copyright file="TestDocumentModule.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using Common;

namespace FatturaFacile.Tests;

/// <summary>
/// Test implementation of <see cref="IDocumentModule"/> used to verify
/// document activation and navigation behaviour within the module manager.
/// </summary>
public class TestDocumentModule : BindableBase, IDocumentModule
{

    /// <summary>
    /// The unique module key used for navigation and identification.
    /// </summary>
    private readonly string key;

    /// <summary>
    /// Indicates whether the module is currently active.
    /// </summary>
    private bool isActive;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDocumentModule"/> class.
    /// </summary>
    /// <param name="key">
    /// The unique module key used for navigation and identification.
    /// </param>
    /// <param name="caption">
    /// The display caption associated with the module.
    /// </param>
    public TestDocumentModule(string key, string caption)
    {
        this.key = key;
        Caption  = caption;
    }

    /// <summary>
    /// Gets the display caption of the module.
    /// </summary>
    public string Caption { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is currently active.
    /// When set to <see langword="true"/>, navigation to the corresponding
    /// document is triggered through the default module manager.
    /// </summary>
    public bool IsActive
    {
        get => isActive;
        set
        {
            if (SetProperty(storage: ref isActive, value: value, propertyName: nameof(IsActive)))
            {
                if (value)
                {
                    ModuleManager.DefaultManager?.Navigate(regionName: Common.Regions.Documents, key: key);
                }
            }
        }
    }

}
