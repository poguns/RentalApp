using StarterApp.ViewModels;
using StarterApp.Services;
using StarterApp.Views;

namespace StarterApp;

public partial class App : Application
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IAuthenticationService _authService;

	public App(IServiceProvider serviceProvider, IAuthenticationService authService)
	{
		_serviceProvider = serviceProvider;
		_authService = authService;
		InitializeComponent();

		Routing.RegisterRoute(nameof(Views.MainPage), typeof(Views.MainPage));
		Routing.RegisterRoute(nameof(Views.LoginPage), typeof(Views.LoginPage));
		Routing.RegisterRoute(nameof(Views.RegisterPage), typeof(Views.RegisterPage));
		Routing.RegisterRoute(nameof(Views.UserListPage), typeof(Views.UserListPage));
		Routing.RegisterRoute(nameof(Views.UserDetailPage), typeof(Views.UserDetailPage));
		Routing.RegisterRoute(nameof(Views.TempPage), typeof(Views.TempPage));
		Routing.RegisterRoute(nameof(Views.ItemsListPage), typeof(Views.ItemsListPage));
		Routing.RegisterRoute(nameof(Views.ItemDetailPage), typeof(Views.ItemDetailPage));
		Routing.RegisterRoute(nameof(Views.CreateEditItemPage), typeof(Views.CreateEditItemPage));
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// var window = base.CreateWindow(activationState);
		// window.Page = new AppShell();

		var shell = _serviceProvider.GetService<AppShell>();
		if (shell == null)
		{
			// Handle the error if AppShell could not be resolved
			throw new InvalidOperationException("AppShell could not be resolved from the service provider.");

		}
		//kick off session restore after window is created
		_ = RestoreSessionAsync();

		var window = new Window(shell);
		return window;
	}

	private async Task RestoreSessionAsync()
    {
        // Small delay so the shell/navigation stack fully initialise
        await Task.Delay(100);

        var restored = await _authService.TryRestoreSessionAsync();

        if (restored)
            await Shell.Current.GoToAsync("//MainPage");
        else
            await Shell.Current.GoToAsync("//LoginPage");
    }
}
