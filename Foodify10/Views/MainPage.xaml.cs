using Foodify10.ViewModels;

namespace Foodify10
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var animationTask = AnimateSkeletonsAsync();
            await _viewModel.InitializeAsync();
            await animationTask;
        }

        private async Task AnimateSkeletonsAsync()
        {
            while (_viewModel.IsArticlesLoading || _viewModel.IsQualityProductsLoading)
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
        }
    }
}