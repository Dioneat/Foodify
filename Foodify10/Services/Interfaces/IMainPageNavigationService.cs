namespace Foodify10.Services.Interfaces
{
    public interface IMainPageNavigationService
    {
        Task OpenShoppingListsAsync();
        Task OpenScanPageAsync();
        Task OpenProductPageAsync(string barcode);
        Task OpenArticlePageAsync(int articleId);
    }
}