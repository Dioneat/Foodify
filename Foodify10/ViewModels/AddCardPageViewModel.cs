using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Foodify10.ViewModels
{
    public partial class AddCardPageViewModel : ObservableObject
    {
        private readonly ICardService _cardService;
        private readonly ICardsNavigationService _navigationService;
        private readonly IAlertService _alertService;

        [ObservableProperty]
        private string cardName = string.Empty;

        [ObservableProperty]
        private string cardNumber = string.Empty;

        [ObservableProperty]
        private string? tempImagePath;

        [ObservableProperty]
        private bool isImagePreviewVisible;

        [ObservableProperty]
        private ImageSource? previewImageSource;

        [ObservableProperty]
        private bool isNumberEntryVisible = true;
        public ObservableCollection<string> AvailableColors { get; } = new()
        {
            "#F9F9F9", // светлый
            "#E8F8F5", // мятный
            "#EBF5FB", // голубой
            "#FEF9E7", // жёлтый
            "#FDEDEC", // розовый
            "#F4ECF7", // сиреневый
            "#EAF2F8", // стальной
            "#E8F6F3"  // зелёно-голубой
        };

        [ObservableProperty]
        private string selectedCardColor = "#F9F9F9";

        public AddCardPageViewModel(
            ICardService cardService,
            ICardsNavigationService navigationService,
            IAlertService alertService)
        {
            _cardService = cardService;
            _navigationService = navigationService;
            _alertService = alertService;
        }

        [RelayCommand]
        private async Task ScanBarcodeAsync()
        {
            var code = await _navigationService.OpenQuickScanAsync();

            if (!string.IsNullOrWhiteSpace(code))
            {
                CardNumber = code;
                TempImagePath = null;
                PreviewImageSource = null;
                IsImagePreviewVisible = false;
                IsNumberEntryVisible = true;
            }
        }

        [RelayCommand]
        private async Task TakePhotoAsync()
        {
            try
            {
                PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Camera>();

                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.Camera>();

                if (status != PermissionStatus.Granted)
                {
                    await _alertService.ShowAlertAsync("Доступ запрещен", "Без доступа к камере нельзя сфотографировать карту", "OK");
                    return;
                }

                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo == null)
                    return;

                TempImagePath = await _cardService.SaveCardImageAsync(photo);
                PreviewImageSource = ImageSource.FromFile(TempImagePath);
                IsImagePreviewVisible = true;
                IsNumberEntryVisible = false;
            }
            catch (FeatureNotSupportedException)
            {
                await _alertService.ShowAlertAsync("Ошибка", "Камера не поддерживается на этом устройстве", "OK");
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Ошибка", $"Произошел сбой: {ex.Message}", "OK");
            }
        }

        [RelayCommand]
        private async Task SaveCardAsync()
        {
            if (string.IsNullOrWhiteSpace(CardName))
            {
                await _alertService.ShowAlertAsync("Ошибка", "Введите название карты", "OK");
                return;
            }

            var newCard = new LoyaltyCard
            {
                Name = CardName,
                AddedDate = DateTime.Now,
                CardColor = SelectedCardColor
            };

            if (!string.IsNullOrEmpty(TempImagePath))
            {
                newCard.IsGenerated = false;
                newCard.ImagePath = TempImagePath;
                newCard.CardNumber = "Отсканированное фото";
            }
            else if (!string.IsNullOrWhiteSpace(CardNumber))
            {
                newCard.IsGenerated = true;
                newCard.CardNumber = CardNumber;
            }
            else
            {
                await _alertService.ShowAlertAsync("Ошибка", "Введите номер или сделайте фото", "OK");
                return;
            }

            await _cardService.SaveCardAsync(newCard);
            await _navigationService.GoBackAsync();
        }
    }
}