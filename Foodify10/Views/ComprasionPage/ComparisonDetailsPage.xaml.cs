using Foodify10.Models;
using Foodify10.ViewModels;

namespace Foodify10;

public partial class ComparisonDetailsPage : ContentPage
{
    private readonly ComparisonDetailsPageViewModel _viewModel;

    public ComparisonDetailsPage(ComparisonDetailsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void Initialize(ComparisonGroup group)
    {
        Title = group.Name;
        _viewModel.Load(group);
    }
}