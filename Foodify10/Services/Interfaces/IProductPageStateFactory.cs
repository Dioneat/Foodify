using Foodify10.Models;
using Foodify10.Models.JsonModels;

namespace Foodify10.Services.Interfaces
{
    public interface IProductPageStateFactory
    {
        ProductPageState CreateSuccessState(ProductData product, string explanationDetails);
        ProductPageState CreateNotFoundState();
        ProductPageState CreateErrorState(string message);
        HistoryItem CreateHistoryItem(string barcode, ProductData product);
        string BuildShareText(ProductPageState state);
    }
}