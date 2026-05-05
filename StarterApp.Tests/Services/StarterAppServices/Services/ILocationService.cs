namespace StarterApp.Services;

public interface ILocationService
{
    Task<(double Latitude, double Longitude)?> GetCurrentLocationAsync();
    Task<double> GetDistanceKmAsync(double lat1, double lon1, double lat2, double lon2);
    bool IsLocationAvailable();
}