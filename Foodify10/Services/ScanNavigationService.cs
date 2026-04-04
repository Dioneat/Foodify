using Foodify10.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Foodify10.Services
{
    public class ScanNavigationService : IScanNavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public ScanNavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OpenProductPageAsync(string barcode)
        {
            var productPage = _serviceProvider.GetRequiredService<ProductPage>();
            productPage.Initialize(barcode);
            await Shell.Current.Navigation.PushAsync(productPage);
        }

        public async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}