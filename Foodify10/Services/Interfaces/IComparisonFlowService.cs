using Foodify10.Models.JsonModels;

namespace Foodify10.Services.Interfaces
{
    public interface IComparisonFlowService
    {
        Task AddProductToComparisonAsync(ProductData product);
    }
}