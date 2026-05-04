namespace StarterApp.Services;

public class LocationService : ILocationService
{
    public async Task<Location?> GetCurrentLocationAsync()
    {
        try
        {
            // Check permission first
            var status = await Permissions
                .RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
                return null;

            var request = new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(10)
            );

            return await Geolocation.Default.GetLocationAsync(request);
        }
        catch (FeatureNotSupportedException)
        {
            // GPS not supported on this device
            return null;
        }
        catch (FeatureNotEnabledException)
        {
            // GPS not enabled
            return null;
        }
        catch (PermissionException)
        {
            // Permission denied
            return null;
        }
        catch (Exception)
        {
            // Any other error
            return null;
        }
    }

    public Task<double> GetDistanceKmAsync(double lat1, double lon1,
                                            double lat2, double lon2)
    {
        var location1 = new Location(lat1, lon1);
        var location2 = new Location(lat2, lon2);

        var distanceKm = location1.CalculateDistance(
            location2, DistanceUnits.Kilometers);

        return Task.FromResult(distanceKm);
    }

    public bool IsLocationAvailable()
    {
        return Geolocation.Default != null;
    }
}