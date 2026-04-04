using Foodify10.ViewModels;

namespace Foodify10;

public partial class MenuPage : ContentPage
{
    public MenuPage(MenuPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}