using Foodify10.Models;
using Foodify10.ViewModels;

namespace Foodify10;

public partial class CardDetailPage : ContentPage
{
    private readonly CardDetailPageViewModel _viewModel;

    public CardDetailPage(CardDetailPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void Initialize(LoyaltyCard card)
    {
        Title = card.Name;
        _viewModel.Load(card);
    }
}