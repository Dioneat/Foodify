using Foodify10.ViewModels;

namespace Foodify10;

public partial class ProductPage : ContentPage
{
    private readonly ProductPageViewModel _viewModel;
    private string? _barcode;

    public ProductPage(ProductPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void Initialize(string barcode)
    {
        _barcode = barcode;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrWhiteSpace(_barcode))
        {
            await _viewModel.LoadAsync(_barcode);
            _barcode = null;
        }
    }
}