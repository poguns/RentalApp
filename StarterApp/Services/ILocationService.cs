namespace StarterApp.Services;

public interface ILocationService
{
    Task<Location?> GetCurrentLocationAsync();
    Task<double> GetDistanceKmAsync(double lat1, double lon1,
                                    double lat2, double lon2);
    bool IsLocationAvailable();
}