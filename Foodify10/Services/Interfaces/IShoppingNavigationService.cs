namespace Foodify10.Services.Interfaces
{
    public interface IShoppingNavigationService
    {
        Task OpenShoppingListAsync(string? listId = null);
        Task OpenShoppingListsAsync();
    }
}