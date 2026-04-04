using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Foodify10.ViewModels
{
    public partial class ShoppingListsPageViewModel : ObservableObject
    {
        private readonly IShoppingListStorageService _storageService;
        private readonly IShoppingNavigationService _navigationService;

        public ObservableCollection<ShoppingListModel> Lists { get; } = new();

        public ShoppingListsPageViewModel(
            IShoppingListStorageService storageService,
            IShoppingNavigationService navigationService)
        {
            _storageService = storageService;
            _navigationService = navigationService;
        }

        public Task LoadAsync()
        {
            Lists.Clear();

            var allLists = _storageService
                .GetAll()
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            foreach (var list in allLists)
                Lists.Add(list);

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task CreateNewListAsync()
        {
            await _navigationService.OpenShoppingListAsync();
        }

        [RelayCommand]
        private async Task OpenListAsync(ShoppingListModel? list)
        {
            if (list == null)
                return;

            await _navigationService.OpenShoppingListAsync(list.Id);
        }
        [RelayCommand]
        private Task DeleteListAsync(ShoppingListModel? list)
        {
            if (list == null)
                return Task.CompletedTask;

            _storageService.Delete(list.Id);
            Lists.Remove(list);

            return Task.CompletedTask;
        }
    }
}