using Foodify10.Services.Interfaces;
using Foodify10.Views;

namespace Foodify10.Services
{
    public class MenuNavigationService : IMenuNavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public MenuNavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OpenBmrCalculatorAsync()
        {
            var page = _serviceProvider.GetRequiredService<BmrPage>();
            await Shell.Current.Navigation.PushAsync(page);
        }

        public async Task OpenDiscountCardsAsync()
        {
            var page = _serviceProvider.GetRequiredService<CardsListPage>();
            await Shell.Current.Navigation.PushAsync(page);
        }

        public async Task OpenShoppingListsAsync()
        {
            await Shell.Current.GoToAsync(nameof(ShoppingListsPage));
        }
        public async Task OpenSettingsAsync()
        {
            var page = _serviceProvider.GetRequiredService<SettingsPage>();
            await Shell.Current.Navigation.PushAsync(page);
        }
        public async Task OpenComparisonAsync()
        {
            var page = _serviceProvider.GetRequiredService<ComparisonListPage>();
            await Shell.Current.Navigation.PushAsync(page);
        }
    }
}