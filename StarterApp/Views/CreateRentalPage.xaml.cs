using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class CreateRentalPage : ContentPage
{
    public CreateRentalPage(CreateRentalViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}