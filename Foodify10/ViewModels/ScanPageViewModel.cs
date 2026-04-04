using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Services.Interfaces;

namespace Foodify10.ViewModels
{
    public partial class ScanPageViewModel : ObservableObject
    {
        private readonly IScanNavigationService _navigationService;
        private readonly IAlertService _alertService;
        private readonly IVibrationService _vibrationService;

        [ObservableProperty]
        private string manualBarcode;

        [ObservableProperty]
        private bool isTorchOn;

        [ObservableProperty]
        private bool isDetecting = true;

        [ObservableProperty]
        private bool isNavigating;

        public string TorchButtonBackgroundColor =>
            IsTorchOn ? "#27AE60" : "#40000000";

        public ScanPageViewModel(
            IScanNavigationService navigationService,
            IAlertService alertService,
            IVibrationService vibrationService)
        {
            _navigationService = navigationService;
            _alertService = alertService;
            _vibrationService = vibrationService;
        }

        partial void OnIsTorchOnChanged(bool value)
        {
            OnPropertyChanged(nameof(TorchButtonBackgroundColor));
        }

        [RelayCommand]
        private void ToggleTorch()
        {
            IsTorchOn = !IsTorchOn;
        }

        [RelayCommand]
        private async Task SubmitManualBarcodeAsync()
        {
            var code = ManualBarcode?.Trim();

            if (string.IsNullOrWhiteSpace(code) || code.Length < 8)
            {
                await _alertService.ShowAlertAsync("Внимание", "Введите корректный штрих-код", "OK");
                return;
            }

            await NavigateToProductAsync(code);
        }

        [RelayCommand]
        private async Task ProcessDetectedBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode) || IsNavigating)
                return;

            await NavigateToProductAsync(barcode);
        }

        [RelayCommand]
        private async Task BackAsync()
        {
            await _navigationService.GoBackAsync();
        }

        private async Task NavigateToProductAsync(string barcode)
        {
            if (IsNavigating)
                return;

            try
            {
                IsNavigating = true;
                IsDetecting = false;

                _vibrationService.Vibrate(100);

                await _navigationService.OpenProductPageAsync(barcode);
            }
            finally
            {
                IsDetecting = true;
                IsNavigating = false;
            }
        }
    }
}