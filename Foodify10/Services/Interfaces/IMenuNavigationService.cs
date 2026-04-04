namespace Foodify10.Services.Interfaces
{
    public interface IMenuNavigationService
    {
        Task OpenBmrCalculatorAsync();
        Task OpenDiscountCardsAsync();
        Task OpenShoppingListsAsync();
        Task OpenComparisonAsync();
        Task OpenSettingsAsync();
    }
}