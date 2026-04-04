using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Services.Interfaces;

namespace Foodify10.ViewModels
{
    public partial class MenuPageViewModel : ObservableObject
    {
        private readonly IMenuNavigationService _navigationService;

        public MenuPageViewModel(IMenuNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task OpenBmrCalculatorAsync()
        {
            await _navigationService.OpenBmrCalculatorAsync();
        }

        [RelayCommand]
        private async Task OpenDiscountCardsAsync()
        {
            await _navigationService.OpenDiscountCardsAsync();
        }
        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            await _navigationService.OpenSettingsAsync();
        }
        [RelayCommand]
        private async Task OpenShoppingListsAsync()
        {
            await _navigationService.OpenShoppingListsAsync();
        }

        [RelayCommand]
        private async Task OpenComparisonAsync()
        {
            await _navigationService.OpenComparisonAsync();
        }
    }
}