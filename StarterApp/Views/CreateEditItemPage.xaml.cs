using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class CreateEditItemPage : ContentPage
{
    public CreateEditItemPage(CreateEditItemViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}