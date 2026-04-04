using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Foodify10.Models;
using Foodify10.Services.Interfaces;
using System.Collections.ObjectModel;

namespace Foodify10.ViewModels
{
    public partial class CardsListPageViewModel : ObservableObject
    {
        private readonly ICardService _cardService;
        private readonly ICardsNavigationService _navigationService;

        public ObservableCollection<LoyaltyCard> Cards { get; } = new();

        [ObservableProperty]
        private LoyaltyCard? selectedCard;

        public CardsListPageViewModel(
            ICardService cardService,
            ICardsNavigationService navigationService)
        {
            _cardService = cardService;
            _navigationService = navigationService;
        }

        partial void OnSelectedCardChanged(LoyaltyCard? value)
        {
            if (value == null)
                return;

            _ = OpenSelectedCardAsync(value);
        }

        public async Task LoadAsync()
        {
            Cards.Clear();

            var cards = await _cardService.GetCardsAsync();
            foreach (var card in cards)
                Cards.Add(card);
        }

        [RelayCommand]
        private async Task AddCardAsync()
        {
            await _navigationService.OpenAddCardAsync();
        }

        [RelayCommand]
        private async Task DeleteCardAsync(LoyaltyCard? card)
        {
            if (card == null)
                return;

            await _cardService.DeleteCardAsync(card.Id);
            Cards.Remove(card);
        }

        private async Task OpenSelectedCardAsync(LoyaltyCard card)
        {
            try
            {
                await _navigationService.OpenCardDetailAsync(card);
            }
            finally
            {
                SelectedCard = null;
            }
        }
    }
}