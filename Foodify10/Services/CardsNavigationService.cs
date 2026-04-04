using Foodify10.Models;
using Foodify10.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Foodify10.Services
{
    public class CardsNavigationService : ICardsNavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public CardsNavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OpenAddCardAsync()
        {
            var page = _serviceProvider.GetRequiredService<AddCardPage>();
            await Shell.Current.Navigation.PushAsync(page);
        }

        public async Task OpenCardDetailAsync(LoyaltyCard card)
        {
            var page = _serviceProvider.GetRequiredService<CardDetailPage>();
            page.Initialize(card);
            await Shell.Current.Navigation.PushAsync(page);
        }

        public async Task OpenFullSizeImageAsync(ImageSource imageSource)
        {
            await Shell.Current.Navigation.PushModalAsync(new FullSizeImagePage(imageSource));
        }

        public async Task CloseModalAsync()
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        public Task GoBackAsync()
        {
            return Shell.Current.Navigation.PopAsync();
        }

        public Task<string?> OpenQuickScanAsync()
        {
            var tcs = new TaskCompletionSource<string?>();

            var scanPage = new QuickScanPage();

            scanPage.OnCodeScanned += code =>
            {
                tcs.TrySetResult(code);
            };

            scanPage.Disappearing += (_, __) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetResult(null);
            };

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.Navigation.PushModalAsync(scanPage);
            });

            return tcs.Task;
        }
    }
}