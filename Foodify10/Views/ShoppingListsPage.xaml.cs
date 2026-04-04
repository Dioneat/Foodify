using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Services;
using Foodify10.ViewModels;

namespace Foodify10.Views;

public partial class ShoppingListsPage : ContentPage
{
    private readonly ShoppingListsPageViewModel _viewModel;

    public ShoppingListsPage(ShoppingListsPageViewModel viewModel)
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