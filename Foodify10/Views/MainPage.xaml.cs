using Foodify10.ViewModels;

namespace Foodify10
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;
        private bool _isAnimating; 

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _isAnimating = true;
            var animationTask = AnimateSkeletonsAsync();

            await _viewModel.InitializeAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _isAnimating = false; 
        }

        private async Task AnimateSkeletonsAsync()
        {
            while (_isAnimating && (_viewModel.IsArticlesLoading || _viewModel.IsQualityProductsLoading))
            {
                try
                {
                    await Task.WhenAll(
                        ArticlesSkeleton.FadeToAsync(0.5, 800),
                        QualitySkeleton.FadeToAsync(0.5, 800)
                    );

                    await Task.WhenAll(
                        ArticlesSkeleton.FadeToAsync(1, 800),
                        QualitySkeleton.FadeToAsync(1, 800)
                    );
                }
                catch
                {
                    break;
                }
            }
        }
    }
}