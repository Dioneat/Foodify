using Foodify10.Models.JsonModels;

namespace Foodify10.Services.Providers
{
    public class ReviewProvider : IReviewProvider
    {
        public Task<ReviewData?> GetReviewsAsync(ProductData p)
        {
            // Пока возвращаем фейковые данные
            return Task.FromResult<ReviewData?>(new ReviewData(new[] { "Хороший продукт", "Часто покупаю" }, 4.8));
        }
    }
}
