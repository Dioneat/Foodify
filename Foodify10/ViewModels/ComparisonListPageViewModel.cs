using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Foodify10.ViewModels
{
    public partial class ComparisonListPageViewModel : ObservableObject
    {
        private readonly IComparisonNavigationService _navigationService;
        private readonly IAlertService _alertService;

        public ObservableCollection<ComparisonGroup> Groups { get; } = new();

        public ComparisonListPageViewModel(
            IComparisonNavigationService navigationService,
            IAlertService alertService)
        {
            _navigationService = navigationService;
            _alertService = alertService;
        }

        public Task LoadAsync()
        {
            ComparisonManager.EnsureLoaded();

            Groups.Clear();

            foreach (var group in ComparisonManager.Groups)
                Groups.Add(group);

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task OpenGroupAsync(ComparisonGroup? group)
        {
            if (group == null)
                return;

            if (group.Products == null || group.Products.Count < 2)
            {
                await _alertService.ShowAlertAsync("Инфо", "Добавьте хотя бы 2 продукта для сравнения", "OK");
                return;
            }

            await _navigationService.OpenComparisonDetailsAsync(group);
        }

        [RelayCommand]
        private Task ClearAllAsync()
        {
            ComparisonManager.Clear();
            Groups.Clear();
            return Task.CompletedTask;
        }
    }
}