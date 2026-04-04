using Foodify10.ViewModels;
using ZXing.Net.Maui;

namespace Foodify10;

public partial class ScanPage : ContentPage
{
    private readonly ScanPageViewModel _viewModel;

    public ScanPage(ScanPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        barcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.Ean13 | BarcodeFormat.DataMatrix,
            TryInverted = true,
            TryHarder = true,
            AutoRotate = true,
            Multiple = false
        };

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ScanPageViewModel.IsTorchOn))
        {
            barcodeReader.IsTorchOn = _viewModel.IsTorchOn;
        }
        else if (e.PropertyName == nameof(ScanPageViewModel.IsDetecting))
        {
            barcodeReader.IsDetecting = _viewModel.IsDetecting;
        }
    }

    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_viewModel.IsNavigating)
            return;

        var first = e.Results.FirstOrDefault();
        if (first == null || string.IsNullOrWhiteSpace(first.Value))
            return;

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await _viewModel.ProcessDetectedBarcodeCommand.ExecuteAsync(first.Value);
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }
}