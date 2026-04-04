using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Services.Interfaces;

namespace Foodify10.ViewModels
{
    public partial class CardDetailPageViewModel : ObservableObject
    {
        private readonly ICardsNavigationService _navigationService;

        private readonly Color _activeColor = Color.FromArgb("#27AE60");
        private readonly Color _inactiveColor = Colors.White;
        private readonly Color _activeTextColor = Colors.White;
        private readonly Color _inactiveTextColor = Colors.Black;

        [ObservableProperty]
        private string cardName = string.Empty;

        [ObservableProperty]
        private string cardNumber = string.Empty;

        [ObservableProperty]
        private bool isGeneratedCodeVisible;

        [ObservableProperty]
        private bool isPhotoVisible;

        [ObservableProperty]
        private string barcodeValue = string.Empty;

        [ObservableProperty]
        private string qrCodeValue = string.Empty;

        [ObservableProperty]
        private ImageSource? cardImageSource;

        [ObservableProperty]
        private bool showBarcode = true;

        public Color BarcodeButtonBackground => ShowBarcode ? _activeColor : _inactiveColor;
        public Color BarcodeButtonTextColor => ShowBarcode ? _activeTextColor : _inactiveTextColor;

        public Color QrButtonBackground => !ShowBarcode ? _activeColor : _inactiveColor;
        public Color QrButtonTextColor => !ShowBarcode ? _activeTextColor : _inactiveTextColor;
        public bool IsBarcodeVisible => ShowBarcode;
        public bool IsQrVisible => !ShowBarcode;

        public CardDetailPageViewModel(ICardsNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        partial void OnShowBarcodeChanged(bool value)
        {
            OnPropertyChanged(nameof(BarcodeButtonBackground));
            OnPropertyChanged(nameof(BarcodeButtonTextColor));
            OnPropertyChanged(nameof(QrButtonBackground));
            OnPropertyChanged(nameof(QrButtonTextColor));
            OnPropertyChanged(nameof(IsBarcodeVisible));
            OnPropertyChanged(nameof(IsQrVisible));
        }

        public void Load(LoyaltyCard card)
        {
            CardName = card.Name;
            CardNumber = card.CardNumber;

            if (card.IsGenerated)
            {
                IsGeneratedCodeVisible = true;
                IsPhotoVisible = false;
                BarcodeValue = card.CardNumber;
                QrCodeValue = card.CardNumber;
                ShowBarcode = true;
            }
            else
            {
                IsGeneratedCodeVisible = false;
                IsPhotoVisible = true;

                if (!string.IsNullOrEmpty(card.ImagePath) && File.Exists(card.ImagePath))
                {
                    CardImageSource = ImageSource.FromFile(card.ImagePath);
                }
            }
        }

        [RelayCommand]
        private void ShowBarcodeTab()
        {
            ShowBarcode = true;
        }

        [RelayCommand]
        private void ShowQrTab()
        {
            ShowBarcode = false;
        }

        [RelayCommand]
        private async Task OpenPhotoAsync()
        {
            if (CardImageSource != null)
            {
                await _navigationService.OpenFullSizeImageAsync(CardImageSource);
            }
        }
    }

}