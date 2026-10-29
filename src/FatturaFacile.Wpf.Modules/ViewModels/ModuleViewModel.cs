// <copyright file="ModuleViewModel.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;

using Common;

namespace FatturaFacile.Wpf.Modules.ViewModels;

/// <summary>
/// Demonstration document module ViewModel supporting serialized state persistence.
/// </summary>
public class ModuleViewModel : IDocumentModule, ISupportState<ModuleViewModel.Info>
{

    /// <summary>
    /// Gets the visual title or caption of the document tab.
    /// </summary>
    public string Caption { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this module document is the currently active tab.
    /// </summary>
    public virtual bool IsActive { get; set; }

    /// <summary>
    /// Gets the collection of demo items.
    /// </summary>
    public ObservableCollection<DataItem> Items { get; private set; }

    /// <summary>
    /// Creates a new instance of <see cref="ModuleViewModel"/> with the specified caption.
    /// </summary>
    /// <param name="caption">The module caption.</param>
    /// <returns>A new <see cref="ModuleViewModel"/> instance created by DevExpress POCO mechanism.</returns>
    public static ModuleViewModel Create(string caption)
    {
        return ViewModelSource.Create(constructorExpression: () => new ModuleViewModel()
                                                                   {
                                                                       Caption = caption,
                                                                   });
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleViewModel"/> class.
    /// </summary>
    protected ModuleViewModel()
    {
        Items = new ObservableCollection<DataItem>();

        Enumerable.Range(start: 0, count: 100)
                  .Select(selector: x => new DataItem()
                                         {
                                             Id    = x,
                                             Value = "Item #" + x.ToString(),
                                         })
                  .ToList()
                  .ForEach(action: x => Items.Add(item: x));
    }

    #region Serialization

    /// <summary>
    /// Encapsulates serialized state information for <see cref="ModuleViewModel"/>.
    /// </summary>
    [Serializable]
    public class Info
    {

        /// <summary>
        /// Gets or sets the serialized caption.
        /// </summary>
        public string Caption { get; set; } = string.Empty;

    }

    Info ISupportState<Info>.SaveState()
    {
        return new Info()
               {
                   Caption = Caption,
               };
    }

    void ISupportState<Info>.RestoreState(Info state) => Caption = state.Caption;

    #endregion

}

/// <summary>
/// Demo data item representing a numerical identifier and text value.
/// </summary>
public class DataItem
{

    /// <summary>
    /// Gets or sets the data item identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the data item text value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

}
