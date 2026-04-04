using Foodify10.ViewModels;

namespace Foodify10;

public partial class ArticlePage : ContentPage, IQueryAttributable
{
    private readonly ArticlePageViewModel _viewModel;

    public ArticlePage(ArticlePageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("articleId", out var raw) &&
            raw != null &&
            int.TryParse(raw.ToString(), out var articleId))
        {
            await _viewModel.LoadAsync(articleId);
        }
    }
}