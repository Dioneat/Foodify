using Foodify10.Models.JsonModels;
using Foodify10.Services.Interfaces;

namespace Foodify10.Services
{
    public class ProductFlowService : IProductFlowService
    {
        private readonly IEnumerable<IProductDataProvider> _dataProviders;
        private readonly IReviewProvider _reviewProvider;
        private readonly ICompositionExplanationProvider _compositionExplanationProvider;

        public ProductFlowService(
            IEnumerable<IProductDataProvider> dataProviders,
            IReviewProvider reviewProvider,
            ICompositionExplanationProvider compositionExplanationProvider)
        {
            _dataProviders = dataProviders;
            _reviewProvider = reviewProvider;
            _compositionExplanationProvider = compositionExplanationProvider;
        }

        public async Task<ProductFinalResult> ProcessProductAsync(ProductSearchRequest request)
        {
            ProductData? productData = null;
            bool isAverageData = false;

            foreach (var provider in _dataProviders)
            {
                productData = await provider.TryGetProductAsync(request);

                if (productData != null)
                    break;
            }

            if (productData == null)
            {
                return new ProductFinalResult(null, null, null);
            }

            var reviewsTask = !isAverageData
                ? _reviewProvider.GetReviewsAsync(productData)
                : Task.FromResult<ReviewData?>(null);

            var explanationTask = _compositionExplanationProvider.GetCompositionExplanationAsync(productData);

            await Task.WhenAll(reviewsTask, explanationTask);

            return new ProductFinalResult(
                productData,
                await reviewsTask,
                await explanationTask);
        }
    }
}