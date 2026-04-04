using Foodify10.ViewModels;

namespace Foodify10;

public partial class CardsListPage : ContentPage
{
    private readonly CardsListPageViewModel _viewModel;

    public CardsListPage(CardsListPageViewModel viewModel)
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