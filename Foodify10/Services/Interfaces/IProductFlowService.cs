using Foodify10.Models.JsonModels;

namespace Foodify10.Services.Interfaces
{
    public interface IProductFlowService
    {
        Task<ProductFinalResult> ProcessProductAsync(ProductSearchRequest request);
    }
}