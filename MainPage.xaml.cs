
using LocationTrackingApp.Internal.Repository;
using Microsoft.Maui.Maps;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace LocationHeatMap
{
    public partial class MainPage : ContentPage
    {

        private LocationRepository _locationRepository = new LocationRepository();
        private IDispatcherTimer _dispatcherTimer;

        public MainPage()
        {
            InitializeComponent();
            StartLocationTracking();
            ShowLocationTracking();
        }

        private async void StartLocationTracking()
        {
            // Request location permissions
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
            {
                _dispatcherTimer = Dispatcher.CreateTimer();
                _dispatcherTimer.Interval = TimeSpan.FromSeconds(5);
                _dispatcherTimer.Tick += async (object? sender, EventArgs e) => await RefreshLocationTimer();
                _dispatcherTimer.Start();
            }
            else
            {
                await DisplayAlert("Permission Denied", "Location permission is required for this feature.", "OK");
            }
        }

        private void ShowLocationTracking()
        {
            // Initialize the location from San Francisco
            //DisplayLocationOnMap(37.7749, -122.4194);
            HeatMap.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HeatMap.VisibleRegion))
                    CanvasView.InvalidateSurface();
            };
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (HeatMap.VisibleRegion == null) return;

            // Set up the paint for drawing circles
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var locations = _locationRepository.GetAllLocations();
            // Instead of looping for all the locations, determine the unique locations and set the density based on the number of locations
            var groupedLocations = locations
                .GroupBy(loc => new { loc.Latitude, loc.Longitude })
                .Select(group => new
                {
                    Latitude = group.Key.Latitude,
                    Longitude = group.Key.Longitude,
                    Count = group.Count()
                })
                .ToList();

            // If there are only one records, the locations won't show for the initial render.
            // Therefore, duplicate the records without corrupting the data
            var samplingLocations = groupedLocations.Concat(groupedLocations);
            foreach (var location in samplingLocations)
            {

                // Convert geographic coordinates to screen position
                var point = ConvertLocationToPixel(location.Latitude, location.Longitude, e.Info.Width, e.Info.Height);

                // Set paint color and transparency based on density
                paint.Color = GetDensityColor(location.Count);
                // Draw heatmap circle
                float radius = 10;
                canvas.DrawCircle(point.X, point.Y, radius, paint);
                DisplayLocationOnMap(location.Latitude, location.Longitude);
            }
        }

        private SKPoint ConvertLocationToPixel(double latitude, double longitude, int width, int height)
        {
            if (HeatMap.VisibleRegion == null) return SKPoint.Empty;
            // Convert latitude/longitude to screen coordinates based on the visible map region
            var region = HeatMap.VisibleRegion;
            var mapLeft = region.Center.Longitude - region.LongitudeDegrees / 2;
            var mapTop = region.Center.Latitude + region.LatitudeDegrees / 2;

            var mapWidth = region.LongitudeDegrees;
            var mapHeight = region.LatitudeDegrees;

            float x = (float)((longitude - mapLeft) / mapWidth * width);
            float y = (float)((mapTop - latitude) / mapHeight * height);

            return new SKPoint(x, y);
        }

        // RefreshLocationTimer will refresh the latest location for every 5 second instead of last known location for accuracy
        // and save to the database to track every moment
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.devices.sensors.geolocation.getlocationasync?view=net-maui-9.0&viewFallbackFrom=net-maui-7.0
        private async Task RefreshLocationTimer() {
            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            Location location = await Geolocation.Default.GetLocationAsync(request, new CancellationTokenSource().Token);
            if (location != null)
            {
                _locationRepository.SaveLocation(location.Latitude, location.Longitude);
            }
        }

        // DisplayLocationOnMap will move the Google Map's focus to the target latitude and longitude
        private void DisplayLocationOnMap(double latitude, double longitude)
        {
            // Displaying on the map (you would need to implement this with MAUI Maps)
            var position = new MapSpan(new Location(latitude, longitude), 0.03, 0.03);
            HeatMap.MoveToRegion(position);
        }

        // GetDensityColor will return the density color based on how many times this location was visited
        // Example: More visits -> Red (higher density)
        private SKColor GetDensityColor(int count)
        {
            if (count > 10) return SKColors.Red;
            if (count > 5) return SKColors.Orange;
            return SKColors.Green;  // Less frequent
        }
    }

}