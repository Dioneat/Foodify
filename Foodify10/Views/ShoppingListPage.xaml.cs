using Foodify10.ViewModels;

namespace Foodify10.Views;

public partial class ShoppingListPage : ContentPage, IQueryAttributable
{
    private readonly ShoppingListPageViewModel _viewModel;
    private bool _queryApplied;

    public ShoppingListPage(ShoppingListPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _queryApplied = true;

        string? listId = null;

        if (query.TryGetValue("listId", out var raw) && raw != null)
            listId = Uri.UnescapeDataString(raw.ToString() ?? string.Empty);

        await _viewModel.LoadAsync(listId);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Загружаем новый список только один раз, если page открыта без параметров
        if (!_queryApplied && !_viewModel.IsLoaded)
        {
            await _viewModel.LoadAsync(null);
        }
    }
}