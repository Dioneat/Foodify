using Foodify10.ViewModels;

namespace Foodify10;

public partial class ComparisonListPage : ContentPage
{
    private readonly ComparisonListPageViewModel _viewModel;

    public ComparisonListPage(ComparisonListPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}