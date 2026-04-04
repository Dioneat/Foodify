using Foodify10.Models;

namespace Foodify10.Services.Interfaces
{
    public interface ICardsNavigationService
    {
        Task OpenAddCardAsync();
        Task OpenCardDetailAsync(LoyaltyCard card);
        Task OpenFullSizeImageAsync(ImageSource imageSource);
        Task CloseModalAsync();
        Task<string?> OpenQuickScanAsync();
        Task GoBackAsync();
    }
}