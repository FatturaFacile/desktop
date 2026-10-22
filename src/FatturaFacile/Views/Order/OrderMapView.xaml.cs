using System.Windows.Controls;
using DevExpress.DevAV.ViewModels;
using DevExpress.Xpf.Map;

namespace DevExpress.DevAV.Views {
    public partial class OrderMapView : UserControl {
        public OrderMapView() {
            InitializeComponent();
        }
        void AzureGeocodeDataProvider_LayerItemsGenerating(object sender, LayerItemsGeneratingEventArgs args) {
            foreach(var pushpinItem in args.Items) {
                MapPushpin pushpin = pushpinItem as MapPushpin;
                if(pushpin != null)
                    ((INavigatorMapViewModel)DataContext).NewPushpinCreated(pushpin);
            }
        }
        void AzureRouteDataProvider_LayerItemsGenerating(object sender, LayerItemsGeneratingEventArgs args) {
            if((args.Items).Length < 3)
                return;
            ((MapPushpin)args.Items[1]).Text = "A";
            ((MapPushpin)args.Items[2]).Text = "B";
            mapControl.ZoomToRegion(((MapPushpin)args.Items[1]).Location, ((MapPushpin)args.Items[2]).Location, 0.4);
        }
    }
}
