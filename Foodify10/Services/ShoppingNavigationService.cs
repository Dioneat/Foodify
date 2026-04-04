using Foodify10.Services.Interfaces;
using Foodify10.Views;

namespace Foodify10.Services
{
    public class ShoppingNavigationService : IShoppingNavigationService
    {
        public Task OpenShoppingListsAsync()
        {
            return Shell.Current.GoToAsync(nameof(ShoppingListsPage));
        }

        public Task OpenShoppingListAsync(string? listId = null)
        {
            if (string.IsNullOrWhiteSpace(listId))
                return Shell.Current.GoToAsync(nameof(ShoppingListPage));

            return Shell.Current.GoToAsync(
                $"{nameof(ShoppingListPage)}?listId={Uri.EscapeDataString(listId)}");
        }
    }
}