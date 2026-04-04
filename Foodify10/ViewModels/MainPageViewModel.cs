using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Collections.ObjectModel;
using static Foodify10.Models.JsonModels.RskrfModels;

namespace Foodify10.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly IHistoryService _historyService;
        private readonly IRoskachestvoService _roskachestvoService;
        private readonly IMainPageNavigationService _navigationService;

        public ObservableCollection<HistoryItem> RecentScans { get; } = new();
        public ObservableCollection<RoskachestvoArticle> Articles { get; } = new();
        public ObservableCollection<QualityProduct> QualityProducts { get; } = new();

        [ObservableProperty]
        private bool isArticlesLoading = true;

        [ObservableProperty]
        private bool isQualityProductsLoading = true;

        [ObservableProperty]
        private bool isInitialized;

        [ObservableProperty]
        private HistoryItem? selectedHistoryItem;

        [ObservableProperty]
        private RoskachestvoArticle? selectedArticle;

        [ObservableProperty]
        private QualityProduct? selectedQualityProduct;

        public bool IsArticlesVisible => !IsArticlesLoading;
        public bool IsQualityProductsVisible => !IsQualityProductsLoading;

        public MainPageViewModel(
            IHistoryService historyService,
            IRoskachestvoService roskachestvoService,
            IMainPageNavigationService navigationService)
        {
            _historyService = historyService;
            _roskachestvoService = roskachestvoService;
            _navigationService = navigationService;
        }

        partial void OnIsArticlesLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsArticlesVisible));
        }

        partial void OnIsQualityProductsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(IsQualityProductsVisible));
        }

        partial void OnSelectedHistoryItemChanged(HistoryItem? value)
        {
            if (value is null)
                return;

            _ = HandleHistorySelectionAsync(value);
        }

        partial void OnSelectedArticleChanged(RoskachestvoArticle? value)
        {
            if (value is null)
                return;

            _ = HandleArticleSelectionAsync(value);
        }

        partial void OnSelectedQualityProductChanged(QualityProduct? value)
        {
            if (value is null)
                return;

            _ = HandleQualityProductSelectionAsync(value);
        }

        public async Task InitializeAsync()
        {
            if (IsInitialized)
            {
                await RefreshHistoryAsync();
                return;
            }

            await LoadAllDataAsync();
            IsInitialized = true;
        }

        public Task RefreshHistoryAsync()
        {
            RecentScans.Clear();

            var savedHistory = _historyService
                .GetHistory()
                .Take(5)
                .ToList();

            foreach (var item in savedHistory)
            {
                RecentScans.Add(item);
            }

            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task OpenShoppingListsAsync()
        {
            await _navigationService.OpenShoppingListsAsync();
        }

        [RelayCommand]
        private async Task OpenScanAsync()
        {
            await _navigationService.OpenScanPageAsync();
        }

        [RelayCommand]
        private async Task DeleteHistoryItemAsync(HistoryItem? item)
        {
            if (item is null)
                return;

            await _historyService.RemoveFromHistoryAsync(item.Id);
            RecentScans.Remove(item);
        }

        private async Task LoadAllDataAsync()
        {
            IsArticlesLoading = true;
            IsQualityProductsLoading = true;

            await RefreshHistoryAsync();

            try
            {
                var articlesTask = _roskachestvoService.GetTipsArticlesForMainPageAsync();
                var productsTask = _roskachestvoService.GetQualityProductsAsync();

                await Task.WhenAll(articlesTask, productsTask);

                Articles.Clear();
                foreach (var article in await articlesTask)
                {
                    Articles.Add(article);
                }

                QualityProducts.Clear();
                foreach (var product in await productsTask)
                {
                    QualityProducts.Add(product);
                }
            }
            finally
            {
                IsArticlesLoading = false;
                IsQualityProductsLoading = false;
            }
        }

        private async Task HandleHistorySelectionAsync(HistoryItem selectedItem)
        {
            try
            {
                await _navigationService.OpenProductPageAsync(selectedItem.Barcode);
            }
            finally
            {
                SelectedHistoryItem = null;
            }
        }

        private async Task HandleArticleSelectionAsync(RoskachestvoArticle article)
        {
            try
            {
                await _navigationService.OpenArticlePageAsync(article.Id);
            }
            finally
            {
                SelectedArticle = null;
            }
        }

        private async Task HandleQualityProductSelectionAsync(QualityProduct product)
        {
            try
            {
                await _navigationService.OpenProductPageAsync(product.Barcode);
            }
            finally
            {
                SelectedQualityProduct = null;
            }
        }
    }
}