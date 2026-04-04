using Foodify10.ViewModels;

namespace Foodify10;

public partial class AddCardPage : ContentPage
{
    public AddCardPage(AddCardPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}