using System;
using System.Linq;
using System.Windows;
using AIDemo.SemanticSearch;
using DevExpress.AIIntegration;
using DevExpress.Xpf.Core;
using Microsoft.Extensions.VectorData;

namespace AIDemo {
    public partial class SemanticSearchModule {
        public SemanticSearchModule() {
            InitializeComponent();
            WarningText = Properties.Resource.EndpointWarning;
            SemanticSearchDataHelper.Load();
            gridControl.ItemsSource = SemanticSearchDataHelper.Movies;
            InitializeVectorStore();
        }

        async void InitializeVectorStore() {
            var sm = SplashScreenManager.CreateWaitIndicator();
            try {
                sm.Show(this);
                var collection = AIExtensionsContainerDesktop.Default.GetVectorCollection<string, VectorStoreRecord>("Movies");
                if(await collection.CollectionExistsAsync())
                    return;
                var records = SemanticSearchDataHelper.Movies.Select((m, index) => new VectorStoreRecord {
                    Key = m.Id.ToString(),
                    Vector = $"\"{m.Title}\" is a {m.Genre.ToLower()} film directed by {m.Directors}. {m.Plot}"
                });
                await collection.EnsureCollectionExistsAsync();
                await collection.UpsertAsync(records);
            }
            catch(Exception ex) {
                DXMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally {
                sm.Close();
            }
        }

        public class VectorStoreRecord {
            [VectorStoreKey]
            public string Key { get; set; }

            [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineDistance)]
            public string Vector { get; set; }
        }
    }
}
