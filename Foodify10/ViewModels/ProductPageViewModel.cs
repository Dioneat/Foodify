using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Models.JsonModels;
using Foodify10.Services;
using Foodify10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Foodify10.ViewModels
{
    public partial class ProductPageViewModel : ObservableObject
    {
        private readonly IProductFlowService _flowService;
        private readonly IHistoryService _historyService;
        private readonly IProductPageStateFactory _stateFactory;
        private readonly IShareService _shareService;
        private readonly IComparisonFlowService _comparisonFlowService;
        private readonly IProductJsonExportService _productJsonExportService;

        private ProductData? _currentProduct;
        private string? _lastLoadedBarcode;

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private string loadingText = "Анализируем состав...";

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string composition = string.Empty;

        [ObservableProperty]
        private string analysisText = string.Empty;

        [ObservableProperty]
        private string calories = "0";

        [ObservableProperty]
        private string proteins = "0";

        [ObservableProperty]
        private string fats = "0";

        [ObservableProperty]
        private string carbs = "0";

        [ObservableProperty]
        private string nutriScore = "-";

        [ObservableProperty]
        private Color nutriScoreBadgeColor = Colors.Gray;

        [ObservableProperty]
        private string nutriExplanation = string.Empty;

        [ObservableProperty]
        private bool isNutriExpanded;

        [ObservableProperty]
        private string novaScore = "-";

        [ObservableProperty]
        private Color novaBadgeColor = Colors.Gray;

        [ObservableProperty]
        private string novaTitle = "NOVA";

        [ObservableProperty]
        private string novaExplanation = string.Empty;

        [ObservableProperty]
        private bool isNovaExpanded;

        [ObservableProperty]
        private Color trafficFatColor = Colors.LightGray;

        [ObservableProperty]
        private Color trafficSugarColor = Colors.LightGray;

        [ObservableProperty]
        private Color trafficSaltColor = Colors.LightGray;

        [ObservableProperty]
        private string trafficExplanation = string.Empty;

        [ObservableProperty]
        private bool isTrafficExpanded;

        [ObservableProperty]
        private bool isAnalysisVisible = true;

        [ObservableProperty]
        private bool hasQualityMark;

        [ObservableProperty]
        private bool isResearchSectionVisible;

        [ObservableProperty]
        private bool isWorthVisible;

        [ObservableProperty]
        private bool isCriteriaVisible;

        [ObservableProperty]
        private string researchSourceTitle = "Данные исследования";

        public ObservableCollection<string> WorthItems { get; } = new();
        public ObservableCollection<ProductCriterionRatingData> CriteriaRatings { get; } = new();

        public bool IsContentVisible => !IsLoading;

        public string NutriArrow => IsNutriExpanded ? "▲" : "▼";
        public string NovaArrow => IsNovaExpanded ? "▲" : "▼";
        public string TrafficArrow => IsTrafficExpanded ? "▲" : "▼";

        public ProductPageViewModel(
            IProductFlowService flowService,
            IHistoryService historyService,
            IProductPageStateFactory stateFactory,
            IShareService shareService,
            IComparisonFlowService comparisonFlowService,
            IProductJsonExportService productJsonExportService)
        {
            _flowService = flowService;
            _historyService = historyService;
            _stateFactory = stateFactory;
            _shareService = shareService;
            _comparisonFlowService = comparisonFlowService;
            _productJsonExportService = productJsonExportService;
        }

        partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(IsContentVisible));
        partial void OnIsNutriExpandedChanged(bool value) => OnPropertyChanged(nameof(NutriArrow));
        partial void OnIsNovaExpandedChanged(bool value) => OnPropertyChanged(nameof(NovaArrow));
        partial void OnIsTrafficExpandedChanged(bool value) => OnPropertyChanged(nameof(TrafficArrow));

        public async Task LoadAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return;

            string normalizedBarcode = BarcodeNormalizationService.NormalizeForProductSearch(barcode);

            if (string.IsNullOrWhiteSpace(normalizedBarcode))
                return;

            if (_lastLoadedBarcode == normalizedBarcode && !string.IsNullOrWhiteSpace(Title))
                return;

            _lastLoadedBarcode = normalizedBarcode;
            IsLoading = true;

            try
            {
                var result = await _flowService.ProcessProductAsync(new ProductSearchRequest(normalizedBarcode));

                if (result?.Product != null)
                {
                    _currentProduct = result.Product;

                    var state = _stateFactory.CreateSuccessState(
                        result.Product,
                        result.Explanation?.Details ?? "Добавки не найдены");

                    ApplyState(state);
                    ApplyResearchData(result.Product);

                    var historyItem = _stateFactory.CreateHistoryItem(normalizedBarcode, result.Product);
                    await _historyService.AddToHistoryAsync(historyItem);
                }
                else
                {
                    _currentProduct = null;
                    ApplyState(_stateFactory.CreateNotFoundState());
                    ClearResearchData();
                }
            }
            catch (Exception ex)
            {
                _currentProduct = null;
                ApplyState(_stateFactory.CreateErrorState(ex.Message));
                ClearResearchData();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ToggleNutri()
        {
            IsNutriExpanded = !IsNutriExpanded;
        }

        [RelayCommand]
        private void ToggleNova()
        {
            IsNovaExpanded = !IsNovaExpanded;
        }

        [RelayCommand]
        private void ToggleTraffic()
        {
            IsTrafficExpanded = !IsTrafficExpanded;
        }

        [RelayCommand]
        private async Task ShareAsync()
        {
            var text = _stateFactory.BuildShareText(new ProductPageState
            {
                Product = _currentProduct,
                Title = Title,
                Composition = Composition,
                Calories = Calories,
                Proteins = Proteins,
                Fats = Fats,
                Carbs = Carbs,
                NutriScore = NutriScore,
                NovaScore = NovaScore,
                NovaTitle = NovaTitle,
                TrafficFatColor = TrafficFatColor,
                TrafficSugarColor = TrafficSugarColor,
                TrafficSaltColor = TrafficSaltColor
            });

            if (!string.IsNullOrWhiteSpace(text))
            {
                await _shareService.ShareTextAsync("Анализ продукта", text);
            }
        }

        [RelayCommand]
        private async Task ExportJsonAsync()
        {
            if (_currentProduct == null)
            {
                await Shell.Current.DisplayAlertAsync("Ошибка", "Нет данных для выгрузки.", "OK");
                return;
            }

            var exportModel = new ProductExportJsonModel
            {
                ExportedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Barcode = _currentProduct.Barcode,
                Title = Title,
                Composition = Composition,
                AnalysisText = AnalysisText,
                Nutrition = new NutritionExportJsonModel
                {
                    Calories = Calories,
                    Proteins = Proteins,
                    Fats = Fats,
                    Carbohydrates = Carbs
                },
                Scores = new ScoresExportJsonModel
                {
                    NutriScore = NutriScore,
                    NutriExplanation = NutriExplanation,
                    NovaScore = NovaScore,
                    NovaTitle = NovaTitle,
                    NovaExplanation = NovaExplanation,
                    TrafficExplanation = TrafficExplanation
                },
                Research = new ResearchExportJsonModel
                {
                    SourceTitle = ResearchSourceTitle,
                    HasQualityMark = HasQualityMark,
                    WorthItems = WorthItems.ToList(),
                    CriteriaRatings = CriteriaRatings
                        .Select(x => new ResearchCriterionExportJsonModel
                        {
                            Title = x.Title,
                            Value = x.Value
                        })
                        .ToList()
                }
            };

            await _productJsonExportService.ExportAndShareAsync(
                exportModel,
                $"{Title}_{_currentProduct.Barcode}");
        }

        [RelayCommand]
        private async Task AddToComparisonAsync()
        {
            if (_currentProduct == null)
            {
                await Shell.Current.DisplayAlertAsync("Загрузка", "Пожалуйста, подождите завершения загрузки данных.", "OK");
                return;
            }

            await _comparisonFlowService.AddProductToComparisonAsync(_currentProduct);
        }

        private void ApplyState(ProductPageState state)
        {
            Title = state.Title;
            Composition = state.Composition;
            AnalysisText = state.AnalysisText;

            Calories = state.Calories;
            Proteins = state.Proteins;
            Fats = state.Fats;
            Carbs = state.Carbs;

            NutriScore = state.NutriScore;
            NutriScoreBadgeColor = state.NutriScoreBadgeColor;
            NutriExplanation = state.NutriExplanation;

            NovaScore = state.NovaScore;
            NovaBadgeColor = state.NovaBadgeColor;
            NovaTitle = state.NovaTitle;
            NovaExplanation = state.NovaExplanation;

            TrafficFatColor = state.TrafficFatColor;
            TrafficSugarColor = state.TrafficSugarColor;
            TrafficSaltColor = state.TrafficSaltColor;
            TrafficExplanation = state.TrafficExplanation;

            IsAnalysisVisible = state.IsAnalysisVisible;

            IsNutriExpanded = false;
            IsNovaExpanded = false;
            IsTrafficExpanded = false;
        }

        private void ApplyResearchData(ProductData product)
        {
            ClearResearchData();

            var research = product.ResearchData;
            if (research == null)
                return;

            ResearchSourceTitle = string.IsNullOrWhiteSpace(product.SourceName)
                ? "Данные исследования"
                : $"Данные: {product.SourceName}";

            HasQualityMark = research.HasQualityMark;

            foreach (var item in research.Worth ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(item))
                    WorthItems.Add(item);
            }

            foreach (var item in research.CriteriaRatings ?? Enumerable.Empty<ProductCriterionRatingData>())
            {
                if (!string.IsNullOrWhiteSpace(item.Title))
                    CriteriaRatings.Add(item);
            }

            IsWorthVisible = WorthItems.Count > 0;
            IsCriteriaVisible = CriteriaRatings.Count > 0;
            IsResearchSectionVisible = HasQualityMark || IsWorthVisible || IsCriteriaVisible;
        }

        private void ClearResearchData()
        {
            WorthItems.Clear();
            CriteriaRatings.Clear();

            HasQualityMark = false;
            IsWorthVisible = false;
            IsCriteriaVisible = false;
            IsResearchSectionVisible = false;
            ResearchSourceTitle = "Данные исследования";
        }
    }
}