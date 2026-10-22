using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Map;

namespace DevExpress.DevAV.ViewModels {
    public interface INavigatorMapViewModel {
        void NewPushpinCreated(MapPushpin newPushpin);
    }
    public class NavigatorMapViewModel<TEntity> : MapViewModelBase, IDocumentContent, INavigatorMapViewModel where TEntity : class {
        public static NavigatorMapViewModel<TEntity> Create(TEntity displayEntity, string startingAddress, GeoPoint startingLocation, string destinationAddress, GeoPoint destinationLocation, Action<Address> applyDestination = null) {
            return ViewModelSource.Create(() => new NavigatorMapViewModel<TEntity>(displayEntity, startingAddress, startingLocation, destinationAddress, destinationLocation, applyDestination));
        }
        const double maximumWalkingDistance = 7.0;
        LinksViewModel linksViewModel;

        protected NavigatorMapViewModel(TEntity displayEntity, string startingAddress, GeoPoint startingLocation, string destinationAddress, GeoPoint destinationLocation, Action<Address> applyDestination) {
            DisplayEntity = displayEntity;
            StartingAddress = startingAddress;
            StartingLocation = startingLocation;
            DestinationAddress = destinationAddress;
            DestinationLocation = destinationLocation;
            CenterPoint = new GeoPoint((startingLocation.Latitude + destinationLocation.Latitude) / 2,
                (startingLocation.Longitude + destinationLocation.Longitude) / 2);
            IsWalkingAvailable = false;
            this.applyDestination = applyDestination;
        }

        public TEntity DisplayEntity { get; private set; }
        public Address Destination { get; set; }
        public bool IsEditingMode { get { return applyDestination != null; } }
        public virtual string StartingAddress { get; set; }
        public virtual GeoPoint StartingLocation { get; set; }
        public virtual string DestinationAddress { get; set; }
        public virtual GeoPoint DestinationLocation { get; set; }
        protected IMapRouteService RouteService { get { return this.GetRequiredService<IMapRouteService>(); } }
        protected IMapPushpinsService MapPushpinsService { get { return this.GetRequiredService<IMapPushpinsService>(); } }
        public virtual List<RoutePoint> ActiveItinerary { get; set; }
        public virtual RoutePoint SelectedItineraryItem { get; set; }
        public virtual GeoPoint CenterPoint { get; set; }
        protected IDocumentManagerService DocumentManagerService { get { return this.GetService<IDocumentManagerService>(); } }
        public virtual bool IsWalkingAvailable { get; set; }
        public virtual bool IsWalkingRoute { get; set; }

        Action<Address> applyDestination;

        public void OnSelectedItineraryItemChanged() {
            if(SelectedItineraryItem != null) {
                CenterPoint = SelectedItineraryItem.Location;
            }
        }

        public void SaveAndClose() {
            applyDestination(Destination);
            Close();
        }
        public bool CanSaveAndClose() {
            return applyDestination != null && Destination != null;
        }

        public void Swap() {
            applyDestination = null;
            this.RaisePropertyChanged(x => x.IsEditingMode);
            string address = StartingAddress;
            StartingAddress = DestinationAddress;
            DestinationAddress = address;
            GeoPoint location = StartingLocation;
            StartingLocation = DestinationLocation;
            DestinationLocation = location;
            CalculateRouteDriving();
        }

        public void CalculateRouteDriving() {
            CalculateRoute(AzureTravelMode.Car);
            IsWalkingRoute = false;
        }

        public void CalculateRouteWalking() {
            CalculateRoute(AzureTravelMode.Pedestrian);
            IsWalkingRoute = true;
        }

        bool isBusy = false;
        void CalculateRoute(AzureTravelMode mode) {
            if(isBusy)
                return;
            isBusy = true;
            var waypoints = new[] { new RouteWaypoint(StartingAddress, StartingLocation), new RouteWaypoint(DestinationAddress, DestinationLocation) };
            RouteService.CalculateRouteAsync(waypoints, mode).ContinueWith(t => {
                if(t.Result.ResultCode == RequestResultCode.Success && t.Result.RouteResults.Count > 0) {
                    AzureRouteResult route = t.Result.RouteResults.First();
                    IsWalkingAvailable = (route.Summary.TravelLengthMeters > maximumWalkingDistance) ? false : true;
                    if(route.Legs.Count > 0) {
                        AzureRouteLeg leg = route.Legs.First();
                        GeoPoint startLocation = leg.Points.First();
                        GeoPoint endLocation = leg.Points.Last();
                        CenterPoint = new GeoPoint((startLocation.Latitude + endLocation.Latitude) / 2, (startLocation.Longitude + endLocation.Longitude) / 2);
                    }
                    ActiveItinerary = route.Guidance.Instructions.Select(item => new RoutePoint(item)).ToList();
                }
                isBusy = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public virtual void OnLoaded() {
            CalculateRouteDriving();
        }

        void INavigatorMapViewModel.NewPushpinCreated(MapPushpin newPushpin) {
            newPushpin.MouseLeftButtonDown += (s, e) => {
                MapPushpin pushpin = s as MapPushpin;
                if((pushpin != null) && (pushpin.State == MapPushpinState.Normal)) {
                    LocationInformation locationInformation = pushpin.Information as LocationInformation;
                    DestinationAddress = locationInformation.Address.FormattedAddress;
                    DestinationLocation = (GeoPoint)pushpin.Location;
                    pushpin.Text = "A";
                    Regex rx = new Regex("(.*?), (.*?), (.*?) (.*)");
                    var match = rx.Match(locationInformation.Address.FormattedAddress);
                    string streetLine = match.Groups[1].ToString().Trim();
                    string city = match.Groups[2].ToString().Trim();
                    string state = match.Groups[3].ToString().Trim();
                    string zipcode = match.Groups[4].ToString().Trim();
                    StateEnum stateEnum = StateEnum.WY;
                    Enum.TryParse(state, out stateEnum);
                    Destination = new Address {
                        City = city,
                        Line = streetLine,
                        State = stateEnum,
                        ZipCode = zipcode,
                        Latitude = DestinationLocation.Latitude,
                        Longitude = DestinationLocation.Longitude
                    };
                    MapPushpinsService.Clear();
                    CalculateRouteDriving();
                }
            };
        }
        public LinksViewModel LinksViewModel {
            get {
                if(linksViewModel == null)
                    linksViewModel = LinksViewModel.Create();
                return linksViewModel;
            }
        }
        [Command(UseCommandManager = false)]
        public void Close() {
            if(DocumentManagerService == null)
                return;
            IDocument document = DocumentManagerService.FindDocument(this);
            if(document != null)
                document.Close();
        }
        void IDocumentContent.OnClose(CancelEventArgs e) { }
        void IDocumentContent.OnDestroy() { }
        IDocumentOwner IDocumentContent.DocumentOwner { get; set; }
        object IDocumentContent.Title {
            get { return "DevAV - " + DestinationAddress; }
        }
    }
    public class RoutePoint {
        public string ManeuverInstruction { get; }
        public string Distance { get; }
        public AzureRouteGuidanceManeuver Maneuver { get; }
        public GeoPoint Location { get; }
        public RoutePoint(AzureRouteInstruction instruction) {
            ManeuverInstruction = instruction.Message;
            const double metersInMile = 1609.344;
            double distanceMiles = instruction.RouteOffsetMeters / metersInMile;
            Distance = (distanceMiles > 0.9) ?
                String.Format("{0:0} mi", Math.Ceiling(distanceMiles)) :
                String.Format("{0:0} ft", Math.Ceiling(distanceMiles * 52.8) * 100);
            Maneuver = instruction.Maneuver;
            Location = instruction.Point;
        }
    }
}
