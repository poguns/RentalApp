using Microsoft.Extensions.Logging;
using StarterApp.ViewModels;
using StarterApp.Database.Data;
using StarterApp.Database.Data.Repositories;
using StarterApp.Views;
using StarterApp.Services;

namespace StarterApp;

public static class MauiProgram
{
    // Set useSharedApi to true to connect to the shared REST API,
    // or false to use the local PostgreSQL database.
    const bool useSharedApi = true;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        //aunthentication
        if (useSharedApi)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://set09102-api.b-davison.workers.dev/")
            };
            builder.Services.AddSingleton(httpClient);
            builder.Services.AddSingleton<IAuthenticationService, ApiAuthenticationService>();
        }
        else
        {
            builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        }

        //items management
        if (useSharedApi)
        {
            // existing HttpClient and auth registration
            builder.Services.AddSingleton<IItemService, ApiItemService>();
        }
        else
        {
            // existing DbContext and auth registration
            builder.Services.AddScoped<IItemRepository, ItemRepository>();
            builder.Services.AddSingleton<IItemService, ItemService>();
        }

        //rental service
        if (useSharedApi)
        {
            builder.Services.AddSingleton<IRentalService, ApiRentalService>();
        }
        else
        {
            builder.Services.AddScoped<IRentalRepository, RentalRepository>();
            builder.Services.AddSingleton<IRentalService, RentalService>();
        }

        //review
        if (useSharedApi)
        {
            builder.Services.AddSingleton<IReviewService, ApiReviewService>();
        }
        else
        {
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddSingleton<IReviewService, ReviewService>();
        }

        builder.Services.AddSingleton<INavigationService, NavigationService>();

        builder.Services.AddSingleton<AppShellViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<App>();

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddSingleton<RegisterViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserDetailPage>();
        builder.Services.AddTransient<UserDetailViewModel>();
        builder.Services.AddSingleton<TempViewModel>();
        builder.Services.AddTransient<TempPage>();
        //items
        builder.Services.AddTransient<ItemsListViewModel>();
        builder.Services.AddTransient<ItemsListPage>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddTransient<CreateEditItemViewModel>();
        builder.Services.AddTransient<CreateEditItemPage>();
        //rentals
        builder.Services.AddTransient<RentalsViewModel>();
        builder.Services.AddTransient<RentalsPage>();
        builder.Services.AddTransient<CreateRentalViewModel>();
        builder.Services.AddTransient<CreateRentalPage>();
        //review
        builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
        builder.Services.AddTransient<ReviewsViewModel>();
        builder.Services.AddTransient<ReviewsPage>();
        //location
        builder.Services.AddSingleton<ILocationService, LocationService>();
        //overdue
        builder.Services.AddSingleton<OverdueService>();



#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}