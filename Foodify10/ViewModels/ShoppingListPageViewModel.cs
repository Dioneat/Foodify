using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Foodify10.ViewModels
{
    public partial class ShoppingListPageViewModel : ObservableObject
    {
        private readonly IShoppingListStorageService _storageService;
        private readonly ISpeechToTextService _speechToTextService;
        private readonly ILocalReminderService _localReminderService;
        private readonly IAlertService _alertService;
        [ObservableProperty]
        private bool isLoaded;
        private ShoppingListModel _currentList = new();

        public ObservableCollection<ShoppingItem> ShoppingItems { get; } = new();

        [ObservableProperty]
        private string listTitle = string.Empty;

        [ObservableProperty]
        private string newItemText = string.Empty;

        [ObservableProperty]
        private DateTime reminderDate = DateTime.Today;

        [ObservableProperty]
        private TimeSpan reminderTime = DateTime.Now.TimeOfDay;

        [ObservableProperty]
        private bool isVoiceRecording;
        private bool _isLoadingData;
        public string VoiceButtonColor => IsVoiceRecording ? "#E74C3C" : "#27AE60";

        public ShoppingListPageViewModel(
            IShoppingListStorageService storageService,
            ISpeechToTextService speechToTextService,
            ILocalReminderService localReminderService,
            IAlertService alertService)
        {
            _storageService = storageService;
            _speechToTextService = speechToTextService;
            _localReminderService = localReminderService;
            _alertService = alertService;

            ShoppingItems.CollectionChanged += OnCollectionChanged;
        }

        partial void OnIsVoiceRecordingChanged(bool value)
        {
            OnPropertyChanged(nameof(VoiceButtonColor));
        }

        partial void OnListTitleChanged(string value)
        {
            AutoSave();
        }

        partial void OnReminderDateChanged(DateTime value)
        {
            AutoSave();
        }

        partial void OnReminderTimeChanged(TimeSpan value)
        {
            AutoSave();
        }

        public Task LoadAsync(string? listId)
        {
            _isLoadingData = true;

            try
            {
                UnsubscribeItems();

                if (string.IsNullOrWhiteSpace(listId))
                {
                    _currentList = new ShoppingListModel();
                    ListTitle = string.Empty;
                    ReminderDate = DateTime.Today;
                    ReminderTime = DateTime.Now.TimeOfDay;

                    ShoppingItems.Clear();
                    IsLoaded = true;
                    return Task.CompletedTask;
                }

                var existing = _storageService.GetById(listId);

                if (existing == null)
                {
                    _currentList = new ShoppingListModel();
                    ListTitle = string.Empty;
                    ReminderDate = DateTime.Today;
                    ReminderTime = DateTime.Now.TimeOfDay;
                    ShoppingItems.Clear();
                    IsLoaded = true;
                    return Task.CompletedTask;
                }

                _currentList = existing;

                ShoppingItems.Clear();

                ListTitle = _currentList.Title;

                if (_currentList.ReminderDate.HasValue)
                {
                    ReminderDate = _currentList.ReminderDate.Value.Date;
                    ReminderTime = _currentList.ReminderDate.Value.TimeOfDay;
                }
                else
                {
                    ReminderDate = DateTime.Today;
                    ReminderTime = DateTime.Now.TimeOfDay;
                }

                foreach (var item in _currentList.Items)
                {
                    SubscribeItem(item);
                    ShoppingItems.Add(item);
                }

                IsLoaded = true;
                return Task.CompletedTask;
            }
            finally
            {
                _isLoadingData = false;
            }
        }

        [RelayCommand]
        private async Task AddItemAsync()
        {
            if (string.IsNullOrWhiteSpace(NewItemText))
                return;

            var item = new ShoppingItem
            {
                Name = NewItemText.Trim(),
                IsBought = false
            };

            SubscribeItem(item);
            ShoppingItems.Add(item);
            NewItemText = string.Empty;

            AutoSave();
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task StartVoiceInputAsync()
        {
            try
            {
                IsVoiceRecording = true;

                var text = await _speechToTextService.ListenOnceAsync("ru-RU");
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var item = new ShoppingItem
                    {
                        Name = text.Trim(),
                        IsBought = false
                    };

                    SubscribeItem(item);
                    ShoppingItems.Add(item);
                    AutoSave();
                }
            }
            catch (Exception ex)
            {
                await _alertService.ShowAlertAsync("Ошибка", ex.Message, "OK");
            }
            finally
            {
                IsVoiceRecording = false;
            }
        }

        [RelayCommand]
        private async Task SetReminderAsync()
        {
            var reminderDateTime = ReminderDate.Date + ReminderTime;

            if (reminderDateTime <= DateTime.Now)
            {
                await _alertService.ShowAlertAsync("Внимание", "Выберите время в будущем", "OK");
                return;
            }

            var enabled = await _localReminderService.EnsureNotificationsEnabledAsync();
            if (!enabled)
            {
                await _alertService.ShowAlertAsync("Ошибка", "Уведомления отключены.", "OK");
                return;
            }

            _currentList.ReminderDate = reminderDateTime;
            AutoSave();

            await _localReminderService.ScheduleReminderAsync(
                (int)(DateTimeOffset.Now.ToUnixTimeSeconds() % int.MaxValue),
                string.IsNullOrWhiteSpace(ListTitle) ? "Список покупок" : ListTitle,
                ShoppingItems.Count > 0
                    ? $"Не забудьте купить: {ShoppingItems.Count} товаров."
                    : "Не забудьте открыть список покупок.",
                reminderDateTime);

            await _alertService.ShowAlertAsync(
                "Готово",
                $"Напоминание установлено на {reminderDateTime:dd.MM.yyyy HH:mm}",
                "OK");
        }

        [RelayCommand]
        private async Task SaveListAsync()
        {
            AutoSave();
            await _alertService.ShowAlertAsync("Готово", "Список сохранён", "OK");
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ShoppingItem item in e.NewItems)
                    SubscribeItem(item);
            }

            if (e.OldItems != null)
            {
                foreach (ShoppingItem item in e.OldItems)
                    UnsubscribeItem(item);
            }

            AutoSave();
        }

        private void SubscribeItem(ShoppingItem item)
        {
            item.PropertyChanged -= OnItemPropertyChanged;
            item.PropertyChanged += OnItemPropertyChanged;
        }

        private void UnsubscribeItem(ShoppingItem item)
        {
            item.PropertyChanged -= OnItemPropertyChanged;
        }

        private void UnsubscribeItems()
        {
            foreach (var item in ShoppingItems)
                UnsubscribeItem(item);
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShoppingItem.IsBought) ||
                e.PropertyName == nameof(ShoppingItem.Name))
            {
                AutoSave();
            }
        }

        private void AutoSave()
        {
            if (_isLoadingData)
                return;

            var hasTitle = !string.IsNullOrWhiteSpace(ListTitle);
            var hasItems = ShoppingItems.Any();

            if (!hasTitle && !hasItems)
                return;

            _currentList.Title = string.IsNullOrWhiteSpace(ListTitle)
                ? "Новый список"
                : ListTitle.Trim();

            _currentList.Items = ShoppingItems.ToList();
            _currentList.ReminderDate = ReminderDate.Date + ReminderTime;

            _storageService.Save(_currentList);
        }
    }
}