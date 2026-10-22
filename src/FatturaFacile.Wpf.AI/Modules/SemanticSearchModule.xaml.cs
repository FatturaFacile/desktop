// <copyright file="SemanticSearchModule.xaml.cs" company="Fattura Facile">
// Copyright (c) Fattura Facile. All rights reserved.
// </copyright>

using System.Windows;
using FatturaFacile.SemanticSearch;
using DevExpress.AIIntegration;
using DevExpress.Xpf.Core;
using Microsoft.Extensions.VectorData;

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
        WarningText = Properties.Resource.EndpointWarning;
        SemanticSearchDataHelper.Load();
        gridControl.ItemsSource = SemanticSearchDataHelper.Movies;
        InitializeVectorStore();
    }

    private async void InitializeVectorStore()
    {
        var sm = SplashScreenManager.CreateWaitIndicator();
        try
        {
            sm.Show(this);
            var collection =
                AIExtensionsContainerDesktop.Default.GetVectorCollection<string, VectorStoreRecord>("Movies");
            if (await collection.CollectionExistsAsync())
            {
                return;
            }

            var records = SemanticSearchDataHelper.Movies.Select((m, index) => new VectorStoreRecord
            {
                Key = m.Id.ToString(),
                Vector = $"\"{m.Title}\" is a {m.Genre.ToLower()} film directed by {m.Directors}. {m.Plot}"
            });
            await collection.EnsureCollectionExistsAsync();
            await collection.UpsertAsync(records);
        }
        catch (Exception ex)
        {
            _ = DXMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        [VectorStoreKey] public string Key { get; set; }

        /// <summary>
        /// Gets or sets textual content transformed into high-dimensional vector representations.
        /// </summary>
        [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineDistance)]
        public string Vector { get; set; }
    }
}
