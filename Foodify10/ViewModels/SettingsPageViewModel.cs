using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Services.Interfaces;

namespace Foodify10.ViewModels
{
    public partial class SettingsPageViewModel : ObservableObject
    {
        private readonly IAlertService _alertService;
        private readonly IHistoryService _historyService;
        private readonly IShoppingListStorageService _shoppingListStorageService;
        private readonly ICardService _cardService;

        private const string ShoppingRemindersKey = "settings_shopping_reminders";
        private const string ScanVibrationKey = "settings_scan_vibration";
        private const string RememberTorchKey = "settings_remember_torch";
        private const string ShoppingAutoSaveKey = "settings_shopping_autosave";

        [ObservableProperty]
        private bool shoppingRemindersEnabled;

        [ObservableProperty]
        private bool scanVibrationEnabled;

        [ObservableProperty]
        private bool rememberTorchState;

        [ObservableProperty]
        private bool shoppingAutoSaveEnabled;

        [ObservableProperty]
        private string appVersionText = string.Empty;

        public SettingsPageViewModel(
            IAlertService alertService,
            IHistoryService historyService,
            IShoppingListStorageService shoppingListStorageService,
            ICardService cardService)
        {
            _alertService = alertService;
            _historyService = historyService;
            _shoppingListStorageService = shoppingListStorageService;
            _cardService = cardService;

            LoadSettings();
            AppVersionText = $"Версия приложения: {AppInfo.Current.VersionString}";
        }

        partial void OnShoppingRemindersEnabledChanged(bool value)
            => Preferences.Default.Set(ShoppingRemindersKey, value);

        partial void OnScanVibrationEnabledChanged(bool value)
            => Preferences.Default.Set(ScanVibrationKey, value);

        partial void OnRememberTorchStateChanged(bool value)
            => Preferences.Default.Set(RememberTorchKey, value);

        partial void OnShoppingAutoSaveEnabledChanged(bool value)
            => Preferences.Default.Set(ShoppingAutoSaveKey, value);

        [RelayCommand]
        private async Task ClearHistoryAsync()
        {
            var confirmed = await ConfirmAsync(
                "Очистить историю",
                "Вы действительно хотите удалить всю историю сканирований?");

            if (!confirmed)
                return;

            _historyService.ClearHistory();
            await _alertService.ShowAlertAsync("Готово", "История сканирований очищена.", "OK");
        }

        [RelayCommand]
        private async Task ClearComparisonsAsync()
        {
            var confirmed = await ConfirmAsync(
                "Очистить сравнения",
                "Вы действительно хотите удалить все списки сравнений?");

            if (!confirmed)
                return;

            ComparisonManager.Clear();
            await _alertService.ShowAlertAsync("Готово", "Списки сравнений очищены.", "OK");
        }

        [RelayCommand]
        private async Task ClearShoppingListsAsync()
        {
            var confirmed = await ConfirmAsync(
                "Удалить списки покупок",
                "Вы действительно хотите удалить все списки покупок? Это действие нельзя отменить.");

            if (!confirmed)
                return;

            _shoppingListStorageService.SaveAll(new List<ShoppingListModel>());
            await _alertService.ShowAlertAsync("Готово", "Все списки покупок удалены.", "OK");
        }

        [RelayCommand]
        private async Task ClearCardsAsync()
        {
            var confirmed = await ConfirmAsync(
                "Удалить карты",
                "Вы действительно хотите удалить все карты лояльности? Это действие нельзя отменить.");

            if (!confirmed)
                return;

            var cards = await _cardService.GetCardsAsync();

            foreach (var card in cards)
            {
                await _cardService.DeleteCardAsync(card.Id);
            }

            await _alertService.ShowAlertAsync("Готово", "Все карты лояльности удалены.", "OK");
        }

        private static Task<bool> ConfirmAsync(string title, string message)
        {
            return Shell.Current.DisplayAlertAsync(title, message, "Да", "Отмена");
        }

        private void LoadSettings()
        {
            ShoppingRemindersEnabled = Preferences.Default.Get(ShoppingRemindersKey, true);
            ScanVibrationEnabled = Preferences.Default.Get(ScanVibrationKey, true);
            RememberTorchState = Preferences.Default.Get(RememberTorchKey, false);
            ShoppingAutoSaveEnabled = Preferences.Default.Get(ShoppingAutoSaveKey, true);
        }
    }
}