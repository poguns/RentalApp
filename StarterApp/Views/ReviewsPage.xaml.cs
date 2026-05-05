using StarterApp.ViewModels;

namespace StarterApp.Views;

public partial class ReviewsPage : ContentPage
{
    private readonly ReviewsViewModel _viewModel;

    public ReviewsPage(ReviewsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadReviewsCommand.Execute(null);
    }
}