// <copyright file="SemanticSearchModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;

using DevExpress.AIIntegration;
using DevExpress.Xpf.Core;

using Microsoft.Extensions.VectorData;
using FatturaFacile.Wpf.AI.Modules.SemanticSearch;
using FatturaFacile.Wpf.AI;

namespace FatturaFacile;

/// <summary>
/// Interaction logic and view module for semantic similarity vector search across tabular movie data using vector collections.
/// </summary>
public partial class SemanticSearchModule
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticSearchModule"/> class and hydrates the underlying vector collection.
    /// </summary>
    public SemanticSearchModule()
    {
        InitializeComponent();
        WarningText = Resource.EndpointWarning;
        SemanticSearchDataHelper.Load();
        gridControl.ItemsSource = SemanticSearchDataHelper.Movies;
        InitializeVectorStore();
    }

    /// <summary>
    /// Initializes and populates the in-memory vector collection if it does not yet exist.
    /// </summary>
    private async void InitializeVectorStore()
    {
        var sm = SplashScreenManager.CreateWaitIndicator();

        try
        {
            sm.Show(owner: this);

            var collection =
                AIExtensionsContainerDesktop.Default.GetVectorCollection<string, VectorStoreRecord>(collectionName: "Movies");

            if (await collection.CollectionExistsAsync())
            {
                return;
            }

            var records = SemanticSearchDataHelper.Movies.Select(selector: (m, index) => new VectorStoreRecord
                                                                                         {
                                                                                             Key = m.Id.ToString(),
                                                                                             Vector =
                                                                                                 $"\"{m.Title}\" is a {m.Genre.ToLower()} film directed by {m.Directors}. {m.Plot}",
                                                                                         });

            await collection.EnsureCollectionExistsAsync();
            await collection.UpsertAsync(records: records);
        }
        catch (Exception ex)
        {
            _ = DXMessageBox.Show(messageBoxText: ex.Message, caption: "Error", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);
        }
        finally
        {
            sm.Close();
        }
    }

    /// <summary>
    /// Demonstrates a vector store record entity with a key and embedding vector.
    /// </summary>
    public class VectorStoreRecord
    {

        /// <summary>
        /// Gets or sets the primary key identifier for the vector record.
        /// </summary>
        [VectorStoreKey]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets textual content transformed into high-dimensional vector representations.
        /// </summary>
        [VectorStoreVector(dimensions: 1536, DistanceFunction = DistanceFunction.CosineDistance)]
        public string Vector { get; set; }

    }

}
