using CommunityToolkit.Maui;
using Foodify10.Models.JsonModels;
using Foodify10.Services;
using Foodify10.Services.Interfaces;
using Foodify10.Services.Providers;
using Foodify10.ViewModels;
using Foodify10.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using ZXing.Net.Maui.Controls;

namespace Foodify10
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLocalNotification()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            RegisterHttpClients(builder.Services);
            RegisterDomainServices(builder.Services);
            RegisterNavigationServices(builder.Services);
            RegisterViewModels(builder.Services);
            RegisterPages(builder.Services);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void RegisterHttpClients(IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddHttpClient<RoskachestvoProvider>();
            services.AddHttpClient<OpenFoodFactsProvider>();
            services.AddHttpClient<NationalCatalogProvider>();
            services.AddHttpClient<YandexEdaProvider>();
        }

        private static void RegisterDomainServices(IServiceCollection services)
        {
            // Product data providers
            services.AddSingleton<IProductDataProvider, RoskachestvoProvider>();
            services.AddSingleton<IProductDataProvider, OpenFoodFactsProvider>();
            services.AddSingleton<IProductDataProvider, NationalCatalogProvider>();
            services.AddSingleton<IProductDataProvider, YandexEdaProvider>();

            // Core business services
            services.AddSingleton<IProductFlowService, ProductFlowService>();
            services.AddSingleton<IReviewProvider, ReviewProvider>();
            services.AddSingleton<IHistoryService, HistoryService>();
            services.AddSingleton<ICardService, CardService>();

            services.AddSingleton<ICompositionExplanationProvider>(_ =>
                new CompositionExplanationProvider("additives.json"));

            // Roskachestvo
            services.AddHttpClient<IRoskachestvoService, RoskachestvoService>(client =>
                  {
                      client.BaseAddress = new Uri("https://rskrf.ru/rest/1/");
                      client.Timeout = TimeSpan.FromSeconds(15);
                  });

            // Shared utility services
            services.AddSingleton<IAlertService, AlertService>();
            services.AddSingleton<IVibrationService, VibrationService>();
            services.AddSingleton<IProductPageStateFactory, ProductPageStateFactory>();
            services.AddSingleton<IShareService, ShareService>();

            // Shopping
            services.AddSingleton<IShoppingListStorageService, ShoppingListStorageService>();
            services.AddSingleton<ISpeechToTextService, SpeechToTextService>();
            services.AddSingleton<ILocalReminderService, LocalReminderService>();

            // Comparison
            services.AddSingleton<IComparisonFlowService, ComparisonFlowService>();
        }

        private static void RegisterNavigationServices(IServiceCollection services)
        {
            services.AddTransient<IMainPageNavigationService, MainPageNavigationService>();
            services.AddTransient<IScanNavigationService, ScanNavigationService>();
            services.AddTransient<IMenuNavigationService, MenuNavigationService>();
            services.AddTransient<IShoppingNavigationService, ShoppingNavigationService>();
            services.AddTransient<ICardsNavigationService, CardsNavigationService>();
            services.AddTransient<IComparisonNavigationService, ComparisonNavigationService>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            services.AddTransient<MainPageViewModel>();
            services.AddTransient<ScanPageViewModel>();
            services.AddTransient<ProductPageViewModel>();
            services.AddTransient<MenuPageViewModel>();
            services.AddTransient<BmrPageViewModel>();
            services.AddTransient<ArticlePageViewModel>();

            services.AddTransient<ShoppingListPageViewModel>();
            services.AddTransient<ShoppingListsPageViewModel>();

            services.AddTransient<CardsListPageViewModel>();
            services.AddTransient<AddCardPageViewModel>();
            services.AddTransient<CardDetailPageViewModel>();

            services.AddTransient<ComparisonListPageViewModel>();
            services.AddTransient<ComparisonDetailsPageViewModel>();

            services.AddTransient<SettingsPageViewModel>();

        }

        private static void RegisterPages(IServiceCollection services)
        {
            services.AddTransient<MainPage>();
            services.AddTransient<ScanPage>();
            services.AddTransient<ProductPage>();
            services.AddTransient<MenuPage>();
            services.AddTransient<BmrPage>();
            services.AddTransient<ArticlePage>();

            services.AddTransient<SettingsPage>();

            services.AddTransient<ShoppingListPage>();
            services.AddTransient<ShoppingListsPage>();

            services.AddTransient<CardsListPage>();
            services.AddTransient<AddCardPage>();
            services.AddTransient<CardDetailPage>();

            services.AddTransient<ComparisonListPage>();
            services.AddTransient<ComparisonDetailsPage>();

            // Эти страницы можно оставить без VM
            services.AddTransient<QuickScanPage>();
            services.AddTransient<FullSizeImagePage>();
        }
    }
}