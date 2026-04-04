using Foodify10.Services.Interfaces;
using Foodify10.Views;

namespace Foodify10.Services
{
    public class MainPageNavigationService : IMainPageNavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IRoskachestvoService _roskachestvoService;

        public MainPageNavigationService(
            IServiceProvider serviceProvider,
            IRoskachestvoService roskachestvoService)
        {
            _serviceProvider = serviceProvider;
            _roskachestvoService = roskachestvoService;
        }

        public async Task OpenShoppingListsAsync()
        {
            await Shell.Current.GoToAsync(nameof(ShoppingListsPage));
        }

        public async Task OpenScanPageAsync()
        {
            var scanPage = _serviceProvider.GetRequiredService<ScanPage>();
            await Shell.Current.Navigation.PushAsync(scanPage);
        }

        public async Task OpenProductPageAsync(string barcode)
        {
            var productPage = _serviceProvider.GetRequiredService<ProductPage>();
            productPage.Initialize(barcode);
            await Shell.Current.Navigation.PushAsync(productPage);
        }

        public async Task OpenArticlePageAsync(int articleId)
        {
            await Shell.Current.GoToAsync($"{nameof(ArticlePage)}?articleId={articleId}");
        }
    }
}