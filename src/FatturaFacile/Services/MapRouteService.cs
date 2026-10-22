using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.DevAV.ViewModels;
using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Map;

namespace DevExpress.DevAV.ViewModels {
    public interface IMapRouteService {
        Task<AzureRouteCalculationResult> CalculateRouteAsync(IEnumerable<RouteWaypoint> waypoints, AzureTravelMode travelMode);
    }
}

namespace DevExpress.DevAV {
    public class MapRouteService : ServiceBase, IMapRouteService {
        InformationLayer Layer { get { return (InformationLayer)AssociatedObject; } }
        AzureRouteDataProvider Provider { get { return (AzureRouteDataProvider)Layer.DataProvider; } }
        TaskCompletionSource<AzureRouteCalculationResult> taskSource;
        public Task<AzureRouteCalculationResult> CalculateRouteAsync(IEnumerable<RouteWaypoint> waypoints, AzureTravelMode travelMode) {
            taskSource = new TaskCompletionSource<AzureRouteCalculationResult>();
            AzureRouteOptions options = new AzureRouteOptions() {
                TravelMode = travelMode,
                InstructionsType = AzureRouteInstructionsType.Text,
            };
            Provider.CalculateRoute(waypoints.ToList(), options);
            Provider.RouteCalculated += Provider_RouteCalculated;
            return taskSource.Task;
        }
        void Provider_RouteCalculated(object sender, AzureRouteCalculatedEventArgs e) {
            Provider.RouteCalculated -= Provider_RouteCalculated;
            if(e.Cancelled)
                taskSource.SetCanceled();
            else if(e.Error != null)
                taskSource.SetException(e.Error);
            else
                taskSource.SetResult(e.CalculationResult);

        }
    }
}
